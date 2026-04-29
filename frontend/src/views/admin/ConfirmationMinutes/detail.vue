<template>
    <div class="minutes-detail-page">
        <button class="back-button" type="button" @click="goBack">
            <i class="pi pi-arrow-left"></i>
            <span>{{ t("client.back_to_list") }}</span>
        </button>

        <div class="detail-grid">
            <section class="document-panel">
                <div class="document-header">
                    <div class="document-name">
                        <i class="pi pi-file"></i>
                        <span>{{ record.fileName }}</span>
                    </div>
                    <Tag
                        :class="getStatusClass(record.statusSeverity)"
                        :value="record.statusKey ? t(record.statusKey) : record.statusLabel"
                    />
                </div>

                <div class="document-preview" :class="{ 'has-preview': canPreviewFile }">
                    <iframe
                        v-if="isPdfFile"
                        class="file-preview-frame"
                        :src="pdfPreviewUrl"
                        :title="record.fileName"
                    ></iframe>
                    <img
                        v-else-if="isImageFile"
                        class="image-preview"
                        :src="resolvedFileUrl"
                        :alt="record.fileName"
                    />
                    <template v-else>
                        <i class="pi pi-file preview-icon"></i>
                        <div class="preview-text">{{ t("client.view_minutes_content") }}</div>
                        <Button
                            class="open-file-button"
                            icon="pi pi-eye"
                            :label="t('client.open_in_new_tab')"
                            outlined
                            :disabled="!record.filePath"
                            @click="openFile"
                        />
                    </template>
                </div>
            </section>

            <aside class="side-panel">
                <section class="info-card">
                    <h4>{{ t("client.minutes_info_title") }}</h4>
                    <div class="info-block">
                        <div class="info-label">{{ t("client.customer_label") }}</div>
                        <div class="info-value">{{ record.customerName }}</div>
                        <div class="info-subvalue">{{ record.customerCode }}</div>
                    </div>
                    <div class="info-block">
                        <div class="info-label">{{ t("client.created_date") }}</div>
                        <div class="info-value">{{ record.createdAt }}</div>
                    </div>
                    <div v-if="record.sentAt" class="info-block">
                        <div class="info-label">{{ t("client.sent_date") }}</div>
                        <div class="info-value">{{ record.sentAt }}</div>
                    </div>
                    <Button
                        v-if="record.canSend"
                        class="send-customer-button"
                        icon="pi pi-send"
                        :label="t('client.sent_to_customer')"
                        :loading="sending"
                        @click="sendMinutes"
                    />
                </section>

                <section class="history-card">
                    <h4>{{ t("client.action_history") }}</h4>
                    <div class="history-list">
                        <div
                            v-for="(item, index) in histories"
                            :key="`${item.title}-${index}`"
                            class="history-item"
                            :class="{ 'last-item': index === histories.length - 1 }"
                        >
                            <span class="history-icon" :class="item.iconClass">
                                <i :class="item.icon"></i>
                            </span>
                            <div class="history-content">
                                <div class="history-title">{{ item.title }}</div>
                                <div class="history-meta">{{ item.actor }} - {{ item.time }}</div>
                                <div v-if="item.note" class="history-note">{{ item.note }}</div>
                            </div>
                        </div>
                    </div>
                </section>
            </aside>
        </div>
        <Loading v-if="loading"></Loading>
    </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import {
    getConfirmationById,
    sendConfirmation,
} from "@/services/confirmation.service";

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const API_URL = import.meta.env.VITE_APP_API || "";

const loading = ref(false);
const sending = ref(false);
const record = ref({
    id: route.params.id,
    fileName: "",
    customerName: "",
    customerCode: "",
    createdAt: "",
    sentAt: "",
    statusKey: "",
    statusLabel: "",
    statusSeverity: "info",
    canSend: false,
});

const notify = (severity, summary, detail) => {
    toast.add({ severity, summary, detail, life: 3000 });
};

const statusClassMap = {
    success: "text-teal-700 bg-teal-200",
    warning: "text-orange-700 bg-white border-1 border-orange-700",
    danger: "text-red-500 border-red-500 bg-white border-1",
    secondary: "text-white bg-gray-500 border-1 border-gray-500",
    info: "text-blue-700 bg-blue-100 border-1 border-blue-200",
};

const getStatusClass = (severity) =>
    `confirmation-status-tag ${statusClassMap[severity] || "text-gray-700 bg-gray-100 border-1 border-gray-200"}`;

const getErrorMessage = (error, fallback = "Đã có lỗi xảy ra") =>
    error?.response?.data?.errors ||
    error?.response?.data?.message ||
    error?.response?.data?.title ||
    error?.message ||
    fallback;

const histories = computed(() => {
    const result = [];

    if (record.value.createdAt) {
        result.push({
            title: t("client.system_upload"),
            actor: record.value.createdBy || "Admin",
            time: record.value.createdAt,
            note: record.value.fileName,
            icon: "pi pi-file",
            iconClass: "file-history-icon",
        });
    }

    if (Array.isArray(record.value.history) && record.value.history.length) {
        result.push(...record.value.history.map((item) => ({
            title: item.titleKey ? t(item.titleKey) : item.title,
            actor: item.actor || "System",
            time: item.time || "",
            note: item.note || "",
            icon: item.icon || "pi pi-file",
            iconClass: item.iconClass || "file-history-icon",
        })));
    }

    return result.reverse();
});

const resolvedFileUrl = computed(() => {
    const filePath = record.value.filePath || "";
    if (!filePath) return "";
    if (/^https?:\/\//i.test(filePath)) return filePath;

    try {
        const apiUrl = new URL(API_URL);
        if (filePath.startsWith("/")) return `${apiUrl.origin}${filePath}`;
        return new URL(filePath, API_URL).toString();
    } catch {
        return filePath;
    }
});

const fileExtension = computed(() => {
    const fileName = record.value.fileName || resolvedFileUrl.value;
    return fileName.split("?")[0].split("#")[0].split(".").pop()?.toLowerCase() || "";
});

const isPdfFile = computed(() => fileExtension.value === "pdf");

const isImageFile = computed(() =>
    ["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"].includes(fileExtension.value)
);

const canPreviewFile = computed(() => !!resolvedFileUrl.value && (isPdfFile.value || isImageFile.value));

const pdfPreviewUrl = computed(() =>
    isPdfFile.value ? `${resolvedFileUrl.value}#toolbar=1&navpanes=0` : ""
);

const fetchDetail = async () => {
    try {
        loading.value = true;
        record.value = await getConfirmationById(route.params.id);
    } catch (error) {
        notify("error", "Lỗi", getErrorMessage(error, "Không thể tải chi tiết biên bản"));
    } finally {
        loading.value = false;
    }
};

const openFile = () => {
    if (!record.value.filePath) {
        notify("warn", "Thông báo", "Biên bản chưa có file đính kèm");
        return;
    }
    window.open(resolvedFileUrl.value, "_blank");
};

const sendMinutes = async () => {
    if (!record.value.id) return;

    try {
        sending.value = true;
        const res = await sendConfirmation(record.value.id, "", record.value);
        notify("success", "Thành công", "Đã gửi biên bản cho khách hàng");
        if (res.notificationError) {
            notify("warn", "Thông báo", "Đã gửi biên bản, nhưng tạo thông báo cho khách hàng thất bại");
        }
        await fetchDetail();
    } catch (error) {
        notify("error", "Lỗi", getErrorMessage(error, "Gửi biên bản thất bại"));
    } finally {
        sending.value = false;
    }
};

const goBack = () => {
    router.push({ name: "admin-confirmation-minutes" });
};

onMounted(fetchDetail);
</script>

<style scoped>
.minutes-detail-page {
    padding: 0.25rem 0 1.5rem;
}

.back-button {
    align-items: center;
    background: transparent;
    border: 0;
    color: #64748b;
    cursor: pointer;
    display: inline-flex;
    font-size: 0.95rem;
    gap: 0.5rem;
    margin-bottom: 1.5rem;
    padding: 0;
}

.back-button:hover {
    color: #334155;
}

.detail-grid {
    align-items: start;
    display: grid;
    gap: 1.75rem;
    grid-template-columns: minmax(0, 1fr) 19rem;
}

.document-panel,
.info-card,
.history-card {
    background: #ffffff;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(15, 23, 42, 0.1);
    overflow: hidden;
}

.document-panel {
    min-height: 38rem;
}

.document-header {
    align-items: center;
    border-bottom: 1px solid #e2e8f0;
    display: flex;
    justify-content: space-between;
    padding: 1rem;
}

.document-name {
    align-items: center;
    color: #0f172a;
    display: flex;
    font-weight: 800;
    gap: 0.6rem;
}

.document-name i {
    color: #4f46e5;
}

:deep(.confirmation-status-tag.p-tag) {
    border-radius: 4px;
    font-size: 0.75rem;
    font-weight: 600;
    line-height: 1;
}

.document-preview {
    align-items: center;
    color: #64748b;
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-height: 31rem;
    padding: 2rem;
    text-align: center;
}

.document-preview.has-preview {
    align-items: stretch;
    justify-content: stretch;
    min-height: 46rem;
    padding: 0;
}

.file-preview-frame {
    border: 0;
    height: 46rem;
    width: 100%;
}

.image-preview {
    height: 46rem;
    object-fit: contain;
    width: 100%;
}

.preview-icon {
    color: #cbd5e1;
    font-size: 3rem;
    margin-bottom: 1.35rem;
}

.preview-text {
    margin-bottom: 1.1rem;
}

.open-file-button {
    border-color: #e2e8f0;
    border-radius: 6px;
    color: #4f46e5;
    font-weight: 700;
}

.side-panel {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

.info-card,
.history-card {
    padding: 1.35rem;
}

.info-card h4,
.history-card h4 {
    color: #0f172a;
    font-size: 1rem;
    font-weight: 800;
    margin: 0 0 1.25rem;
}

.info-block {
    margin-bottom: 1rem;
}

.info-label {
    color: #64748b;
    font-size: 0.75rem;
    font-weight: 800;
    letter-spacing: 0.05em;
    margin-bottom: 0.35rem;
    text-transform: uppercase;
}

.info-value {
    color: #0f172a;
    font-weight: 800;
    line-height: 1.35;
}

.info-subvalue {
    color: #475569;
    font-size: 0.85rem;
    margin-top: 0.1rem;
}

.send-customer-button {
    background: #4f46e5;
    border-color: #4f46e5;
    border-radius: 6px;
    font-weight: 800;
    margin-top: 0.15rem;
    width: 100%;
}

.history-list {
    display: flex;
    flex-direction: column;
}

.history-item {
    display: grid;
    gap: 0.8rem;
    grid-template-columns: 1.4rem minmax(0, 1fr);
    padding-bottom: 1.35rem;
    position: relative;
}

.history-item::before {
    background: #e2e8f0;
    bottom: 0.05rem;
    content: "";
    left: 0.68rem;
    position: absolute;
    top: 1.4rem;
    width: 1px;
}

.history-item.last-item {
    padding-bottom: 0;
}

.history-item.last-item::before {
    display: none;
}

.history-icon {
    align-items: center;
    background: #3b82f6;
    border-radius: 999px;
    color: #ffffff;
    display: inline-flex;
    height: 1.4rem;
    justify-content: center;
    width: 1.4rem;
    z-index: 1;
}

.history-icon i {
    font-size: 0.75rem;
}

.file-history-icon {
    background: #3b82f6;
}

.send-history-icon {
    background: #6366f1;
}

.approve-history-icon {
    background: #059669;
}

.reject-history-icon {
    background: #dc2626;
}

.history-title {
    color: #0f172a;
    font-weight: 800;
    line-height: 1.2;
}

.history-meta {
    color: #64748b;
    font-size: 0.8rem;
    margin-top: 0.15rem;
}

.history-note {
    background: #f8fafc;
    border-radius: 6px;
    color: #64748b;
    font-size: 0.78rem;
    font-style: italic;
    margin-top: 0.45rem;
    padding: 0.55rem 0.7rem;
}

@media (max-width: 992px) {
    .detail-grid {
        grid-template-columns: 1fr;
    }

    .document-panel {
        min-height: 26rem;
    }
}
</style>
