<template>
    <div>
        <Dialog v-model:visible="visible" @hide="onHide" :header="t('client.title')" modal style="width: 60rem">
            <!-- <div>
                {{ modelValue }}
            </div>
            <div>
                {{ selection.map((item) => item.id) }}
            </div> -->
            <DataTable id="userTable" v-model:selection="selection" :selectionMode="props.selectionMode"
                :scrollable="true" scrollHeight="400px" :value="dataTable.data || []" :totalRecords="dataTable.total"
                stripedRows showGridlines keys="id">
                <template #header>
                    <div class="flex align-items-center justify-content-end">
                        <IconField iconPosition="left">
                            <InputIcon class="pi pi-search"> </InputIcon>
                            <InputText @input="handleSearchInput" v-model="query.search"
                                :placeholder="t('body.home.search_placeholder')" />
                        </IconField>
                    </div>
                </template>
                <template #empty>
                    <div class="flex align-items-center justify-content-center" style="height: 330px">
                        <template v-if="loading">{{ t('body.report.loading_message') }}</template>
                        <template v-else-if="dataTable.data.length < 1">
                            <template v-if="query.search">{{ t('body.promotion.no_matching_result_message')
                                }}</template>
                            <template v-else>{{ t('body.promotion.no_data_message') }}</template>
                        </template>
                    </div>
                </template>
                <Column :pt="{
                    headerContent: {
                        class: 'hidden',
                    },
                }" :selectionMode="props.selectionMode" class="w-1rem">
                </Column>
                <Column :header="t('client.User')">
                    <template #body="{ data }">
                        <div class="flex gap-3 align-items-center">
                            <div class="flex">
                                <Avatar :label="`${data.fullName[0]}`.toUpperCase()" size="large" class="font-bold"
                                    shape="circle"></Avatar>
                            </div>
                            <div>
                                <div class="flex align-items-center">
                                    <span class="font-bold mr-2">{{
                                        data.fullName
                                    }}</span>
                                    <Tag v-if="0" :value="data.role?.name"></Tag>
                                </div>
                                <div class="text-500">{{ data.email }}</div>
                            </div>
                        </div>
                    </template>
                </Column>
                <Column field="role.name" :header="t('Navbar.menu.roles')"></Column>
            </DataTable>
            <template #footer>
                <div class="flex justify-content-between align-items-center flex-grow-1">
                    <div class="flex">
                        <template v-if="selection">
                            <span class="my-auto">{{ t('body.productManagement.selected_label') }} {{ selection?.length
                                }}
                            </span>
                            <Button @click="selection = []" icon="pi pi-times" text severity="danger" />
                        </template>
                    </div>
                    <div class="flex gap-2">
                        <Button @click="visible = false" :label="t('client.cancel')" severity="secondary" />
                        <Button @click="onClickConfirm" :label="t('client.select')" />
                    </div>
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick } from "vue";
import { useInfiniteScroll } from "@vueuse/core";
import { debounce } from "lodash";
import API from "../../api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

interface Props {
    selectionMode?: "multiple" | "single";
    notIncludedIds?: Array<number>;
}

const PATH = "account/getall";
const tableElement = ref();
const visible = ref(false);
const selection = ref<Array<any>>([]);
const loading = ref(false);
const error = ref(false);
const dataTable = reactive({
    data: [] as Array<any>,
    total: 0,
});

const emits = defineEmits(["change"]);
const modelValue = defineModel<Array<number>>();
const props = withDefaults(defineProps<Props>(), {
    selectionMode: "multiple",
    notIncludedIds: () => [],
});

const query = reactive({
    skip: 0,
    limit: 30,
    search: "",
    userType: "APSP",
    OrderBy: "id desc",
    Filter: "(status=A)",
    toQueryString: () => {
        let result: Array<string> = [];
        Object.keys(query).forEach((key: string) => {
            if (key != "toQueryString") result.push(`${key}=${query[key]}`);
        });
        return "?" + result.join("&");
    },
});

const onClickConfirm = () => {
    emits("change", selection.value);
    switch (props.selectionMode) {
        case "multiple":
            modelValue.value = selection.value.map((user: any) => user.id);
            break;
        case "single":
            modelValue.value = [selection.value[0]];
            break;
        default:
            console.error("Invalid selection mode");
            return;
    }
    visible.value = false;
};

const onSearch = debounce(() => {
    loading.value = false;
    error.value = false;
    dataTable.data = [];
    dataTable.total = 0;
    query.skip = 0;
    fetchUsers(query.toQueryString());
}, 500);

const handleSearchInput = () => {
    onSearch();
};

const fetchUsers = (_queryString: string) => {
    loading.value = true;
    API.get(`${PATH}${_queryString}`)
        .then((res) => {
            if (res.data.item?.length < 1) {
                error.value = true;
                return;
            }
            if (props.selectionMode == "multiple" && modelValue.value?.length) {
                const selectedUsers: Array<any> = res.data.item?.filter(
                    (user: any) => {
                        return (
                            modelValue.value?.includes(user.id) &&
                            !selection.value.map((su) => su.id).includes(user.id)
                        );
                    }
                    // selection.value.includes((su: any) => su.id !== user.id)
                );
                selection.value.push(...selectedUsers);
            } else if (props.selectionMode == "single" && modelValue.value) {
                const selectedUser = res.data.item?.find(
                    (user: any) => user.id == modelValue.value
                );
                if (selectedUser) {
                    selection.value = selectedUser;
                }
            }
            dataTable.data = [
                ...dataTable.data,
                ...res.data.item
            ];
            dataTable.total = res.data.total;
        })
        .catch((error) => {
            error.value = true;
            console.error(error);
        })
        .finally(() => {
            loading.value = false;
        });
};

const onHide = () => {
    selection.value = [];
    visible.value = false;
    loading.value = false;
    error.value = false;
    dataTable.data = [];
    dataTable.total = 0;
    query.search = "";
    query.skip = 0;
};

const open = () => {
    visible.value = true;
    nextTick(() => {
        tableElement.value = document.querySelector(
            "#userTable > div.p-datatable-wrapper"
        );
    });
};

useInfiniteScroll(
    tableElement,
    () => {
        if (loading.value || error.value) return;
        fetchUsers(query.toQueryString());
        query.skip++;
    },
    {
        distance: 10,
    }
);

const initialComponent = () => {
    // code here
    if (!modelValue.value) {
        modelValue.value = [];
    }
};

defineExpose({
    open,
});

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
