const status = {
    P: {
        severity: "warning",
        label: "Chờ xác nhận",
    },
    A: {
        severity: "success",
        label: "Đã xác nhận",
    },
    R: {
        severity: "danger",
        label: "Từ chối",
    },
    C: {
        severity: "secondary",
        label: "Đã hủy",
    },
};

export function getStatus(str: string): { severity: string; label: string } {
    return (
        status[str] || {
            severity: "contrast",
            label: "Unknown",
        }
    );
}