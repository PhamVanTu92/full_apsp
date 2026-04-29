export default function authourization(e) {
    let user = null;
    try {
        user = JSON.parse(localStorage.getItem('user'));
    } catch {
        // corrupted storage — treat as unauthenticated
    }

    if (!user) {
        return e.router.push({ name: 'login' });
    }

    const userType = user['appUser']?.['userType'];
    if (userType === 'APSP') {
        return e;
    } else if (userType === 'NPP') {
        return e.router.push({ name: 'client-home' });
    } else {
        return e.router.push({ name: 'login' });
    }
}
