import API from "@/api/api-main";
import formatHelper from "@/helpers/format.helper";

const getCaseInsensitiveValue = (source, key) => {
    if (!source || typeof source !== "object") return undefined;
    if (Object.prototype.hasOwnProperty.call(source, key)) return source[key];

    const matchedKey = Object.keys(source).find((sourceKey) => sourceKey.toLowerCase() === key.toLowerCase());
    return matchedKey ? source[matchedKey] : undefined;
};

const getFirstValue = (source, keys, fallback = "") => {
    for (const key of keys) {
        const value = getCaseInsensitiveValue(source, key);
        if (value !== undefined && value !== null && value !== "") return value;
    }
    return fallback;
};

const normalizeStatusCode = (status) => {
    if (status === undefined || status === null || status === "") return "";
    return String(status).trim();
};

const statusMap = {
    0: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    1: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    2: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    3: { label: "Từ chối", severity: "danger", key: "client.reject" },
    A: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    APPROVE: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    APPROVED: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    CONFIRM: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    CONFIRMED: { label: "Đã xác nhận", severity: "success", key: "client.confirmed" },
    P: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    S: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    SENT: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    WAITING: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    PENDING: { label: "Chờ xác nhận", severity: "warning", key: "client.waiting_confirmation" },
    R: { label: "Từ chối", severity: "danger", key: "client.reject" },
    REJECT: { label: "Từ chối", severity: "danger", key: "client.reject" },
    REJECTED: { label: "Từ chối", severity: "danger", key: "client.reject" },
    C: { label: "Đã hủy", severity: "secondary", key: "" },
    CANCEL: { label: "Đã hủy", severity: "secondary", key: "" },
    CANCELLED: { label: "Đã hủy", severity: "secondary", key: "" },
    D: { label: "Chưa gửi", severity: "info", key: "" },
    N: { label: "Chưa gửi", severity: "info", key: "" },
    NEW: { label: "Chưa gửi", severity: "info", key: "" },
    DRAFT: { label: "Chưa gửi", severity: "info", key: "" },
};

export const getConfirmationStatus = (status) => {
    const code = normalizeStatusCode(status);
    const config = statusMap[code.toUpperCase()];

    return (
        config || {
            label: code || "Chưa gửi",
            severity: "info",
            key: "",
        }
    );
};

const formatDateTime = (value) => {
    if (!value) return "";
    if (Number.isNaN(new Date(value).getTime())) return value;
    return formatHelper.DateTime(value)?.dateTime || "";
};

const getHistoryActionConfig = (action) => {
    const normalizedAction = normalizeStatusCode(action).toUpperCase();
    const actionMap = {
        SEND: { label: "Gửi cho khách hàng", key: "client.sent_to_customer", icon: "pi pi-send", iconClass: "send-history-icon" },
        SENT: { label: "Gửi cho khách hàng", key: "client.sent_to_customer", icon: "pi pi-send", iconClass: "send-history-icon" },
        APPROVE: { label: "Khách hàng xác nhận", key: "", icon: "pi pi-check", iconClass: "approve-history-icon" },
        APPROVED: { label: "Khách hàng xác nhận", key: "", icon: "pi pi-check", iconClass: "approve-history-icon" },
        CONFIRM: { label: "Khách hàng xác nhận", key: "", icon: "pi pi-check", iconClass: "approve-history-icon" },
        CONFIRMED: { label: "Khách hàng xác nhận", key: "", icon: "pi pi-check", iconClass: "approve-history-icon" },
        REJECT: { label: "Khách hàng từ chối", key: "", icon: "pi pi-times", iconClass: "reject-history-icon" },
        REJECTED: { label: "Khách hàng từ chối", key: "", icon: "pi pi-times", iconClass: "reject-history-icon" },
    };

    return actionMap[normalizedAction] || {
        label: action || "Lịch sử thao tác",
        key: "",
        icon: "pi pi-file",
        iconClass: "file-history-icon",
    };
};

const getActionTimeValue = (item) => {
    const actionDate = getFirstValue(item, ["actionDateRaw", "actionDate", "time", "createdAt"]);
    const timeValue = new Date(actionDate).getTime();
    return Number.isNaN(timeValue) ? 0 : timeValue;
};

const normalizeConfirmationHistory = (history = []) => {
    if (!Array.isArray(history)) return [];

    return history.map((item) => {
        const action = getFirstValue(item, ["action", "status", "title"]);
        const actionCode = normalizeStatusCode(action).toUpperCase();
        const actionConfig = getHistoryActionConfig(action);
        const actionDate = getFirstValue(item, ["actionDate", "time", "createdAt"]);

        return {
            ...item,
            actionCode,
            title: getFirstValue(item, ["title"], actionConfig.label),
            titleKey: actionConfig.key,
            actor: getFirstValue(item, ["actor", "actionBy", "createdBy", "userName"], "System"),
            time: formatDateTime(actionDate),
            actionDateRaw: actionDate,
            note: getFirstValue(item, ["note", "comment", "remarks"]),
            icon: actionConfig.icon,
            iconClass: actionConfig.iconClass,
        };
    });
};

const getRawItems = (responseData) => {
    if (Array.isArray(responseData)) return responseData;
    if (Array.isArray(responseData?.items)) return responseData.items;
    if (Array.isArray(responseData?.data?.items)) return responseData.data.items;
    if (Array.isArray(responseData?.data)) return responseData.data;
    if (responseData && typeof responseData === "object") return [responseData];
    return [];
};

export const getResponseTotal = (responseData, fallbackLength = 0) =>
    getFirstValue(responseData, ["total", "totalRecords", "count"], fallbackLength);

export const normalizeConfirmationRecord = (record = {}) => {
    const status = normalizeStatusCode(getFirstValue(record, ["status", "docStatus", "confirmationStatus"]));
    const sentDate = getFirstValue(record, ["sentAt", "sentDate", "sendingDate", "sendDate"]);
    const createdDate = getFirstValue(record, ["createdAt", "createdDate", "createDate", "docDate"]);
    const approvedDate = getFirstValue(record, ["approvedAt", "approvedDate", "confirmationDate"]);
    const rejectedDate = getFirstValue(record, ["rejectedAt", "rejectedDate"]);
    const filePath = getFirstValue(record, ["filePath", "fileUrl", "url", "path", "attachmentUrl"]);
    const history = normalizeConfirmationHistory(getFirstValue(record, ["history", "histories", "logs"], []));
    const sentActions = ["SEND", "SENT"];
    const approvedActions = ["APPROVE", "APPROVED", "CONFIRM", "CONFIRMED"];
    const rejectedActions = ["REJECT", "REJECTED"];
    const latestApprovedHistory = history
        .filter((item) => approvedActions.includes(item.actionCode))
        .sort((a, b) => getActionTimeValue(b) - getActionTimeValue(a))[0];
    const latestRejectedHistory = history
        .filter((item) => rejectedActions.includes(item.actionCode))
        .sort((a, b) => getActionTimeValue(b) - getActionTimeValue(a))[0];
    const latestSentHistory = history
        .filter((item) => sentActions.includes(item.actionCode))
        .sort((a, b) => getActionTimeValue(b) - getActionTimeValue(a))[0];
    const latestResponseHistory = [latestApprovedHistory, latestRejectedHistory]
        .filter(Boolean)
        .sort((a, b) => getActionTimeValue(b) - getActionTimeValue(a))[0];
    const resolvedSentDate = sentDate || latestSentHistory?.actionDateRaw || "";
    const resolvedApprovedDate = approvedDate || (latestResponseHistory?.actionCode && approvedActions.includes(latestResponseHistory.actionCode) ? latestResponseHistory.actionDateRaw : "");
    const resolvedRejectedDate = rejectedDate || (latestResponseHistory?.actionCode && rejectedActions.includes(latestResponseHistory.actionCode) ? latestResponseHistory.actionDateRaw : "");
    const derivedStatus = resolvedApprovedDate ? "2" : resolvedRejectedDate ? "3" : resolvedSentDate ? "1" : "0";
    const statusConfig = getConfirmationStatus(derivedStatus);

    return {
        ...record,
        id: getFirstValue(record, ["id", "confirmationId"]),
        code: getFirstValue(record, ["code", "docCode", "documentCode", "confirmationCode", "cardCode"]),
        fileName: getFirstValue(record, ["fileName", "name", "documentName", "attachmentName"], "Biên bản xác nhận"),
        filePath,
        customerName: getFirstValue(record, ["customerName", "cardName", "bpName"]),
        customerCode: getFirstValue(record, ["customerCode", "cardCode", "bpCode"]),
        cardId: getFirstValue(record, ["cardId", "customerId", "bpId"], 0),
        createdBy: getFirstValue(record, ["createdBy", "creatorName", "createdUserName"]),
        createdAt: formatDateTime(createdDate),
        createdDate,
        sentAt: formatDateTime(resolvedSentDate),
        sentDate: resolvedSentDate,
        approvedAt: formatDateTime(resolvedApprovedDate),
        rejectedAt: formatDateTime(resolvedRejectedDate),
        status: derivedStatus,
        rawStatus: status,
        statusKey: statusConfig.key,
        statusLabel: statusConfig.label,
        statusSeverity: statusConfig.severity,
        canSend: !resolvedSentDate && !resolvedApprovedDate && !resolvedRejectedDate,
        canRespond: !!resolvedSentDate && !resolvedApprovedDate && !resolvedRejectedDate,
        note: getFirstValue(record, ["note", "remarks", "comment"]),
        history,
    };
};

export const normalizeConfirmationList = (responseData) =>
    getRawItems(responseData).map((item) => normalizeConfirmationRecord(item));

const buildQuery = ({ page = 1, pageSize = 100, filter = "", search = "" } = {}) => {
    const params = new URLSearchParams();
    params.append("Page", page);
    params.append("PageSize", pageSize);
    if (filter) params.append("Filter", filter);
    if (search) params.append("search", search);
    return params.toString();
};

export const getConfirmations = async (query) => {
    const res = await API.get(`Confirmation/getall?${buildQuery(query)}`);
    return {
        raw: res.data,
        items: normalizeConfirmationList(res.data),
        total: getResponseTotal(res.data, normalizeConfirmationList(res.data).length),
    };
};

export const getConfirmationById = async (id) => {
    const res = await API.get(`Confirmation/${id}`);
    return normalizeConfirmationRecord(res.data?.data || res.data);
};

export const createConfirmation = async ({ file, customer, note = "" }) => {
    const formData = new FormData();
    formData.append("File", file);
    formData.append("CardId", customer?.id || customer?.cardId || 0);
    formData.append("CardCode", customer?.cardCode || customer?.customerCode || "");
    formData.append("CardName", customer?.cardName || customer?.customerName || "");
    formData.append("Note", note || "");
    return await API.add("Confirmation/create", formData);
};

export const createConfirmationNotification = async (record = {}) => {
    const id = getFirstValue(record, ["id", "confirmationId"]);
    const fileName = getFirstValue(record, ["fileName", "name"], "biên bản xác nhận");
    const customerName = getFirstValue(record, ["customerName", "cardName"]);
    const cardId = getFirstValue(record, ["cardId", "customerId", "bpId"], 0);
    const cardCode = getFirstValue(record, ["customerCode", "cardCode", "bpCode"]);

    const payload = {
        title: "Biên bản xác nhận cần phản hồi",
        message: `Bạn có ${fileName} cần xác nhận${customerName ? ` từ ${customerName}` : ""}.`,
        objType: "confirmation_minutes",
        objId: id,
        object: {
            objType: "confirmation_minutes",
            objId: id,
        },
        cardId,
        customerId: cardId,
        bpId: cardId,
        cardCode,
        customerCode: cardCode,
        bpCode: cardCode,
        cardName: customerName,
        customerName,
        fileName,
    };

    try {
        return await API.add("notifications/send", payload);
    } catch (error) {
        // Some deployments expose this endpoint as a string body binder.
        return await API.add("notifications/send", JSON.stringify(JSON.stringify(payload)));
    }
};

export const sendConfirmation = async (id, note = "", record = {}) => {
    const res = await API.add("Confirmation/send", { id, note });
    const notificationRecord = {
        ...record,
        id: record?.id || id,
    };

    try {
        await createConfirmationNotification(notificationRecord);
    } catch (error) {
        res.notificationError = error;
    }

    return res;
};

export const approveConfirmation = async (id, note = "") =>
    await API.add("Confirmation/approve", { id, note });

export const rejectConfirmation = async (id, note = "") =>
    await API.add("Confirmation/reject", { id, note });
