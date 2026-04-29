const formatNoti = {
    formatStatus: (_status, t) => { // Accept t as a parameter
        const STATUS_CONFIG = {
            exchange: { label: t("body.status.changePromotion"), class: "primary" },
            order: { label: t("body.status.order"), class: "primary" },
            approval: { label: t("body.status.approval"), class: "info" },
            order_request: { label: t("body.status.order_request"), class: "info" },
            sale_forecast: { label: t("body.status.sale_forecast"), class: "info" },
            feebycustomer: { label: t("body.status.feebycustomer"), class: "warning" },
            commited: { label: t("body.status.commited"), class: "warning" },
            system: { label: t("body.status.system"), class: "contrast" },
            debt_reconciliation: { label: t("body.status.debt_reconciliation"), class: "danger" },
            rating: { label: t("body.status.rating"), class: "success" },
            confirmation_minutes: { label: t("client.confirmation_minutes"), class: "info" },
        }
        return (
            STATUS_CONFIG[_status] || { // Access directly, not with .value
                label: t("Notification.undefined_notification"),
                class: "contrast",
            }
        );
    }
}
export default formatNoti;
