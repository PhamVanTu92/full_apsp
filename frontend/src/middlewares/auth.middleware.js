export default function auth({ from, next, router, to }) {
    let user = null;
    try {
        user = JSON.parse(localStorage.getItem('user'));
    } catch {
        // corrupted storage — treat as unauthenticated
    }

    if (!user) {
        return router.push({ name: 'login' });
    }

    if (user['user']?.userType == 'APSP' && to.path == '/client/order') {
        return router.push({ name: 'login', query: { goto: to.path } });
    }

    return next();
}
