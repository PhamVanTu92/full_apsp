<template>
    <Loading v-if="loading" />
    <h3 class="uppercase font-bold text-2xl mb-2">{{ t('notificationPage.title') }}</h3>
    <div>
        <DataTable
            stripedRows
            class="table-main"
            showGridlines
            dataKey="id"
            :value="notifications"
            tableStyle="min-width: 50rem;"
            header="surface-200"
            paginator
            :first="limit * page"
            :rows="limit"
            :page="page"
            :totalRecords="totalRecords"
            @page="onPageChange($event)"
            :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)"
            lazy
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        >
            <template #header>
                <div class="flex justify-content-between">
                    <IconField iconPosition="left">
                        <InputIcon class="pi pi-search" />
                        <InputText v-model="keySearch" :placeholder="t('notificationPage.search_placeholder')" @keyup.enter="fetchNotifications" class="w-30rem" />
                    </IconField>  
                    <Dropdown v-model="selectedRead" :options="readOption" optionLabel="name" optionValue="code" placeholder="Select Status" />
                    <!-- <Button @click="visible = true" :label="t('notificationPage.settings_button')" icon="fa-solid fa-gear" /> -->
                </div>
            </template>
            <Column field="created_at" style="width: 13%" :header="t('notificationPage.time_column')">
                <template #body="{ data }">
                    <span class="border-1 border-300 py-1">
                        <span class="surface-300 px-3 p-1">{{ format.DateTime(data.notification.createdAt).time }}</span>
                        <span class="px-3 p-1">
                            {{ format.DateTime(data.notification.createdAt).date }}
                        </span>
                    </span>
                </template>
            </Column>
            <Column :header="t('notificationPage.category_column')">
                <template #body="{ data }">
                    <Tag class="text-xs cursor-pointer" :severity="formatNoti.formatStatus(data.notification.object.objType, t).class" :value="formatNoti.formatStatus(data.notification.object.objType, t).label" />
                </template>
            </Column>
            <Column field="notification.title" :header="t('notificationPage.function_column')" />
            <Column field="notification.message" :header="t('notificationPage.content_column')"></Column>
            <Column class="w-12rem" :header="t('notificationPage.status_column')">
                <template #body="{ data }">
                    <Tag :severity="!data.isRead ? 'warning' : 'primary'" :value="data.isRead ? t('notificationPage.status_read') : t('notificationPage.status_unread')"></Tag>
                </template>
            </Column>
            <Column style="width: 3rem">
                <template #body="{ data }">
                    <div class="flex justify-content-between">
                        <Button icon="pi pi-eye" text v-tooltip.left="'Xem chi tiết'" @click="changePageNoti(data)" />
                        <Button @click="toggleMenuOption($event, data)" text icon="fa-solid fa-ellipsis-vertical" />
                    </div>
                </template>
            </Column>
        </DataTable>
    </div>

    <Dialog v-model:visible="visible" :header="t('notificationPage.settings_dialog_title')" :draggable="false" modal style="width: 50rem">
        <div class="p-5 h-5rem text-center text-yellow-700">
            {{ t('notificationPage.feature_under_development') }}
        </div>
        <template #footer>
            <Button :label="t('common.close_button')" severity="secondary" icon="pi pi-times" />
            <Button :label="t('common.save_button')" icon="pi pi-save" />
        </template>
    </Dialog>
    <Menu ref="menuOption" id="overlay_menu" :model="menu" :popup="true" />
</template>

<script setup>
import { ref, onBeforeMount, computed } from 'vue'; // Import computed
import API from '@/api/api-main';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';
import format from '@/helpers/format.helper';
import formatNoti from '@/helpers/formatStatusNotification.helper';
import { useRouter } from 'vue-router';

const router = useRouter();
const toast = useToast();
const { t } = useI18n();
const menuOption = ref();
const visible = ref(false);
const loading = ref(false);
const notifications = ref([]);
const page = ref(0);
const limit = ref(10);
const totalRecords = ref(0);
const notiSelector = ref();
const keySearch = ref('');
const selectedRead = ref('all')
const readOption = ref([
    { name: 'Tất cả', code: 'all' },
    { name: 'Đã xem', code: 'true' },  
    { name: 'Chưa xem', code: 'false' },  
]);
// begin - static function -----------------------------------
const fetchNotifications = async () => {
    loading.value = true;
    try {
        const res = await API.get(`Notification?skip=${page.value}&limit=${limit.value}&search=${keySearch.value}`); 
        notifications.value = res.data.data;
        totalRecords.value = res.data.total;
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: t('common.error'),
            detail: t('notificationPage.load_failed_prefix') + error.message,
            life: 3000
        });
    } finally {
        loading.value = false;
    }
};
const initialComponent = async () => {
    await fetchNotifications();
};
const toggleMenuOption = (event, data) => {
    menuOption.value.toggle(event);
    notiSelector.value = data;
};

const menu = computed(() => {
    if (!notiSelector.value) return [];
    return [
        {
            label: notiSelector.value.isRead ? t('notificationPage.mark_as_unread') : t('notificationPage.mark_as_read'),
            icon: 'fa-regular fa-circle-check',
            command: () => {
                if (notiSelector.value.isRead) {
                    notiSelector.value.isRead = false;
                    API.add('Notification/' + notiSelector.value.id + '/unread');
                } else {
                    notiSelector.value.isRead = true;
                    API.add('Notification/' + notiSelector.value.id + '/read');
                }
            }
        }
    ];
});
const onPageChange = (event) => {
    page.value = event.page;
    limit.value = event.rows;
    fetchNotifications();
};

const changePageNoti = (data) => {
    let objType = data.notification.object.objType;
    let id = data.notification.object.objId;
    if (!data.isRead) {
        data.isRead = true;
        API.add('Notification/' + data.id + '/read');
    }
    switch (objType) {
        case 'order':
            router.push({ name: 'order-detail', params: { id: id } });
            break;
        case 'order_request':
            router.push({ name: 'DetailPurchaseRequest', params: { id: id } });
            break;
        case 'approval':
            router.push({ name: 'orderApproval', params: { id: id } });
            break;
        case 'feebycustomer':
            router.push({ name: 'warehouse-storage-fee', params: { id: id } });
            break;
        case 'sale_forecast':
            router.push({ name: 'orderPlanningDetail', params: { id: id } });
            break;
        case 'commited':
            router.push({ name: 'commited-outputing', params: { id: id } });
            break;
        case 'exchange':
            router.push({ name: 'change_point_edit', params: { id: id } });
            break;
        case 'confirmation_minutes':
            router.push({ name: 'client-confirmation-minutes-detail', params: { id: id } });
            break;
        default:
            break;
    }
};
onBeforeMount(async () => {
    await initialComponent();
});
</script>

<style></style>
