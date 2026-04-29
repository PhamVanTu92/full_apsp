export default {
    install(app, { i18n }) {
        app.config.globalProperties.$notify = (status, message, toast) => {
            const t = i18n.global.t;
            switch (status) {
                case 'S':
                    toast.add({
                        severity: 'success',
                        summary: t('Custom.titleMessageSuccess'),
                        detail: message,
                        life: 3000
                    });
                    break;
                case 'I':
                    toast.add({ severity: 'info', summary: t('Custom.titleMessageInfo'), detail: message, life: 3000 });
                    break;
                case 'W':
                    toast.add({ severity: 'warn', summary: t('Custom.titleMessageWarning'), detail: message, life: 3000 });
                    break;
                case 'E':
                    toast.add({
                        severity: 'error',
                        summary: t('Custom.titleMessageError'),
                        detail: message,
                        life: 3000
                    });
                    break;
                default:
                    console.error('Invalid status provided to Notification');
            }
        };
    }
};
