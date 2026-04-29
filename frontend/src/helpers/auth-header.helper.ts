type AuthHeaders = Record<string, string>;

function getToken(): string | null {
    const user = localStorage.getItem('user');
    if (!user) return null;
    try {
        return JSON.parse(user)?.token ?? null;
    } catch {
        return null;
    }
}

export function authHeader(): AuthHeaders {
    const token = getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

export function authHeader_new(): AuthHeaders {
    return authHeader();
}
