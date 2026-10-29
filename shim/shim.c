/*
 * Native Notepad++ plugin shim (MinGW-w64, x64).
 * Exports the functions Notepad++ looks for, starts the .NET runtime through
 * hostfxr, and forwards calls to MarkdownPreviewPlugin.dll (class Exports).
 */
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdint.h>
#include <stdio.h>
#include <wchar.h>

#define EXPORT __declspec(dllexport)

typedef struct { void *npp, *main, *second; } NppData;

typedef int (*fxr_init_fn)(const wchar_t *runtime_config, const void *params, void **handle);
typedef int (*fxr_get_delegate_fn)(void *handle, int type, void **delegate);
typedef int (*fxr_close_fn)(void *handle);
typedef int (*load_asm_fn)(const wchar_t *asm_path, const wchar_t *type_name, const wchar_t *method_name,
                           const wchar_t *delegate_type_name, void *reserved, void **delegate);

typedef void  (*set_info_fn)(NppData);
typedef void *(*funcs_fn)(void *);
typedef void  (*notify_fn)(void *);

static set_info_fn p_setInfo;
static funcs_fn    p_getFuncs;
static notify_fn   p_notify;
static int         g_state = 0; /* 0 = not tried, 1 = ok, -1 = failed */

static void module_dir(wchar_t *dir)
{
    HMODULE h = NULL;
    GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                       (LPCWSTR)&module_dir, &h);
    GetModuleFileNameW(h, dir, MAX_PATH);
    wchar_t *s = wcsrchr(dir, L'\\');
    if (s) *s = 0;
}

/* Finds <root>\host\fxr\<highest version>\hostfxr.dll */
static int newest_fxr(const wchar_t *root, wchar_t *out)
{
    wchar_t pat[MAX_PATH];
    WIN32_FIND_DATAW fd;
    HANDLE h;
    long long best = -1;
    wchar_t bestName[64] = L"";

    _snwprintf(pat, MAX_PATH, L"%ls\\host\\fxr\\*", root);
    pat[MAX_PATH - 1] = 0;
    h = FindFirstFileW(pat, &fd);
    if (h == INVALID_HANDLE_VALUE) return 0;
    do {
        int a = 0, b = 0, c = 0;
        if (!(fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) continue;
        if (swscanf(fd.cFileName, L"%d.%d.%d", &a, &b, &c) < 1) continue;
        long long key = ((long long)a << 40) | ((long long)b << 20) | (long long)c;
        if (key > best) {
            best = key;
            wcsncpy(bestName, fd.cFileName, 63);
            bestName[63] = 0;
        }
    } while (FindNextFileW(h, &fd));
    FindClose(h);
    if (best < 0) return 0;

    _snwprintf(out, MAX_PATH, L"%ls\\host\\fxr\\%ls\\hostfxr.dll", root, bestName);
    out[MAX_PATH - 1] = 0;
    return GetFileAttributesW(out) != INVALID_FILE_ATTRIBUTES;
}

static int try_root(const wchar_t *root, wchar_t *fxr)
{
    wchar_t r[MAX_PATH];
    size_t n;
    if (!root || !root[0]) return 0;
    wcsncpy(r, root, MAX_PATH - 1);
    r[MAX_PATH - 1] = 0;
    n = wcslen(r);
    while (n > 0 && (r[n - 1] == L'\\' || r[n - 1] == L'/')) r[--n] = 0;
    return newest_fxr(r, fxr);
}

static int find_hostfxr(wchar_t *fxr)
{
    wchar_t buf[MAX_PATH], tmp[MAX_PATH];
    HKEY k;

    if (GetEnvironmentVariableW(L"DOTNET_ROOT", buf, MAX_PATH) > 0 && try_root(buf, fxr)) return 1;

    if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\dotnet\\Setup\\InstalledVersions\\x64", 0,
                      KEY_READ | KEY_WOW64_64KEY, &k) == ERROR_SUCCESS) {
        DWORD type = 0, sz = sizeof(buf) - sizeof(wchar_t);
        buf[0] = 0;
        if (RegQueryValueExW(k, L"InstallLocation", NULL, &type, (LPBYTE)buf, &sz) == ERROR_SUCCESS &&
            type == REG_SZ) {
            buf[sz / sizeof(wchar_t)] = 0;
            RegCloseKey(k);
            if (try_root(buf, fxr)) return 1;
        } else {
            RegCloseKey(k);
        }
    }

    if (GetEnvironmentVariableW(L"ProgramFiles", tmp, MAX_PATH) > 0) {
        _snwprintf(buf, MAX_PATH, L"%ls\\dotnet", tmp);
        if (try_root(buf, fxr)) return 1;
        _snwprintf(buf, MAX_PATH, L"%ls\\dotnet\\x64", tmp);
        if (try_root(buf, fxr)) return 1;
    }
    if (GetEnvironmentVariableW(L"ProgramFiles(x86)", tmp, MAX_PATH) > 0) {
        _snwprintf(buf, MAX_PATH, L"%ls\\dotnet\\x64", tmp);
        if (try_root(buf, fxr)) return 1;
    }
    return 0;
}

static int fail(const wchar_t *msg, int rc)
{
    wchar_t text[600];
    if (rc) _snwprintf(text, 600, L"%ls\n\nCode: 0x%08X", msg, (unsigned)rc);
    else    _snwprintf(text, 600, L"%ls", msg);
    text[599] = 0;
    MessageBoxW(NULL, text, L"SuperMarkdown++", MB_OK | MB_ICONERROR);
    return 0;
}

static int ensure(void)
{
    wchar_t dir[MAX_PATH], cfg[MAX_PATH], asmp[MAX_PATH], fxrp[MAX_PATH];
    HMODULE fxr;
    fxr_init_fn init;
    fxr_get_delegate_fn get_delegate;
    fxr_close_fn close_fn;
    load_asm_fn load;
    void *ctx = NULL, *d = NULL;
    int rc;
    const wchar_t *type = L"MarkdownPreviewPlugin.Exports, MarkdownPreviewPlugin";
    const wchar_t *uco = (const wchar_t *)(intptr_t)-1; /* UNMANAGEDCALLERSONLY_METHOD */

    if (g_state) return g_state > 0;
    g_state = -1;

    module_dir(dir);
    _snwprintf(cfg, MAX_PATH, L"%ls\\MarkdownPreviewPlugin.runtimeconfig.json", dir);
    _snwprintf(asmp, MAX_PATH, L"%ls\\MarkdownPreviewPlugin.dll", dir);
    cfg[MAX_PATH - 1] = asmp[MAX_PATH - 1] = 0;

    if (GetFileAttributesW(cfg) == INVALID_FILE_ATTRIBUTES || GetFileAttributesW(asmp) == INVALID_FILE_ATTRIBUTES)
        return fail(L"MarkdownPreviewPlugin.dll / .runtimeconfig.json missing next to the plugin DLL. "
                    L"Copy the whole build output folder.", 0);

    if (!find_hostfxr(fxrp))
        return fail(L"x64 .NET runtime (hostfxr.dll) not found. Install the .NET 8 Desktop Runtime (x64).", 0);

    fxr = LoadLibraryW(fxrp);
    if (!fxr) return fail(L"Could not load hostfxr.dll.", (int)GetLastError());

    init = (fxr_init_fn)GetProcAddress(fxr, "hostfxr_initialize_for_runtime_config");
    get_delegate = (fxr_get_delegate_fn)GetProcAddress(fxr, "hostfxr_get_runtime_delegate");
    close_fn = (fxr_close_fn)GetProcAddress(fxr, "hostfxr_close");
    if (!init || !get_delegate || !close_fn) return fail(L"hostfxr.dll is missing required exports.", 0);

    rc = init(cfg, NULL, &ctx);
    if (rc < 0 || !ctx)
        return fail(L"Failed to initialize the .NET runtime. Is the .NET 8 Desktop Runtime (x64) installed?", rc);

    rc = get_delegate(ctx, 5 /* hdt_load_assembly_and_get_function_pointer */, &d);
    close_fn(ctx);
    if (rc < 0 || !d) return fail(L"Failed to get the .NET assembly loader.", rc);
    load = (load_asm_fn)d;

    rc = load(asmp, type, L"SetInfo", uco, NULL, (void **)&p_setInfo);
    if (rc < 0) return fail(L"Failed to bind Exports.SetInfo.", rc);
    rc = load(asmp, type, L"GetFuncsArray", uco, NULL, (void **)&p_getFuncs);
    if (rc < 0) return fail(L"Failed to bind Exports.GetFuncsArray.", rc);
    rc = load(asmp, type, L"BeNotified", uco, NULL, (void **)&p_notify);
    if (rc < 0) return fail(L"Failed to bind Exports.BeNotified.", rc);

    g_state = 1;
    return 1;
}

/* ---- exports Notepad++ expects ---- */

EXPORT BOOL isUnicode(void)
{
    return TRUE;
}

EXPORT const wchar_t *getName(void)
{
    return L"SuperMarkdown++";
}

EXPORT void setInfo(NppData data)
{
    if (ensure()) p_setInfo(data);
}

EXPORT void *getFuncsArray(int *nbF)
{
    if (!ensure()) {
        *nbF = 0;
        return NULL;
    }
    return p_getFuncs(nbF);
}

EXPORT void beNotified(void *notification)
{
    if (g_state > 0) p_notify(notification);
}

EXPORT LRESULT messageProc(UINT msg, WPARAM wParam, LPARAM lParam)
{
    (void)msg; (void)wParam; (void)lParam;
    return TRUE;
}
