<template>
    <div>
        <InputGroup>
            <AutoComplete
                v-model="customerSelected"
                @complete="search"
                :suggestions="suggestion"
                :placeholder="props.placeholder"
                class="w-full"
                optionLabel="cardName"
                completeOnFocus
                @item-select="onItemSelect"
                @change="onChangeAutoComplt"
                :invalid="props.invalid"
                :disabled="props.disabled"
            >
                <template #option="sp">
                    <div class="flex w-full">
                        <div
                            class="w-3rem h-3rem surface-300 flex align-items-center justify-content-center border-circle"
                        >
                            <i class="pi pi-user text-lg"></i>
                        </div>
                        <div class="ml-3">
                            <div class="mb-1 font-bold">{{ sp.option.cardName }}</div>
                            <div>{{ sp.option.email }}</div>
                        </div>
                    </div>
                </template>
                <template #empty>
                    <div class="text-center py-5">{{ t('common.no_results') }}</div>
                </template>
            </AutoComplete>
            <Button
                :disabled="props.disabled"
                @click="onClickShowDlg"
                severity="secondary"
                icon="pi pi-bars"
            />
        </InputGroup>
        <Dialog
            @hide="onClickCancelSelect"
            :header="t('common.dialog_select_customer')"
            v-model:visible="visible"
            modal
            class="w-5"
        >
            <!-- :loading="loading.dataTable" -->
            <div class="relative">
                <DataTable
                    v-model:selection="customerSelection"
                    :value="customerDataTable?.items || []"
                    selectionMode="single"
                    scrollable
                    scrollHeight="350px"
                    id="customerDt"
                    :pt="{
                        thead: {
                            class: 'hidden',
                        },
                    }"
                >
                    <Column selectionMode="single" class="w-1rem"></Column>
                    <!-- <Column header="#">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column> -->
                    <Column header="#">
                        <template #body="sp">
                            <div class="flex w-full">
                                <div
                                    class="w-3rem h-3rem surface-300 flex align-items-center justify-content-center border-circle"
                                >
                                    <i class="pi pi-user text-lg"></i>
                                </div>
                                <div class="ml-3">
                                    <div class="mb-1 font-bold">
                                        {{ sp.data.cardName }}
                                    </div>
                                    <div>{{ sp.data.email }}</div>
                                </div>
                            </div>
                        </template>
                    </Column>
                    <template #empty>
                        <div
                            class="text-center text-500 font-italic"
                            style="height: 320px"
                        >
                            <div class="my-auto" v-if="!enableLoad">{{ t('common.no_data') }}</div>
                            <div class="my-auto" v-if="loadError">{{ t('common.msg_error_occurred') }}</div>
                        </div>
                    </template>
                </DataTable>
                <div
                    v-if="loading.dataTable"
                    class="flex justify-content-center align-items-center absolute surface-200 top-0 bottom-0 left-0 right-0 bg-black-alpha-30"
                >
                    <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                </div>
            </div>
            <template #footer>
                <Button
                    @click="onClickCancelSelect"
                    :label="t('common.btn_cancel')"
                    icon="pi pi-times"
                    severity="secondary"
                />
                <Button
                    @click="onClickConfirmSelect"
                    :label="t('common.btn_select')"
                    icon="pi pi-check"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import AutoComplete from "primevue/autocomplete";
import { ref, reactive, computed, onMounted, nextTick, watch } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
import API from "../api/api-main";
import { useInfiniteScroll } from "@vueuse/core";

const props = defineProps<{
    placeholder?: string;
    invalid?: boolean;
    disabled?: boolean;
}>();

const modelValue = defineModel();

const emits = defineEmits(["item-select"]);

const suggestion = ref([] as any);

const customerSelected = ref();

const onChangeAutoComplt = (e: any): void => {
    if (typeof e.value == "string") {
        // customerSelected.value = null;
        modelValue.value = null;
    }
};

const search = async (e: any): Promise<void> => {
    const query = e.query.trim();
    API.get(`customer?search=${query}`).then((response) => {
        suggestion.value = response.data.items;
    });
};

const onItemSelect = (e: any): void => { 
    emits("item-select", e.value.id);
    modelValue.value = e.value.id;
};

// Dialog logic -------------------------------------------------------
const visible = ref<boolean>(false);
const scrollElement = ref();
const customerDataTable = ref<CustomerResponse>({} as CustomerResponse);
const customerSelection = ref();
const onClickShowDlg = async (): Promise<void> => {
    customerDataTable.value.items = [];
    customerDataTable.value.skip = -1;
    customerDataTable.value.limit = 30;
    customerDataTable.value.total = 0;
    loading.dataTable = false;
    enableLoad.value = true;
    loadError.value = false;
    // customerDataTable.value = await getCustomers("");
    visible.value = true;
    nextTick(() => {
        scrollElement.value = document.querySelector(
            "#customerDt > div.p-datatable-wrapper"
        );
    });
};
const onClickCancelSelect = (): void => {
    customerSelection.value = null;
    visible.value = false;
};
const onClickConfirmSelect = (): void => {
    customerSelected.value = customerSelection.value;
    visible.value = false;
    emits("item-select", customerSelected.value.id);
    modelValue.value = customerSelected.value.id;
};

const loading = reactive({
    dataTable: false,
});
var enableLoad = ref(true);
const loadError = ref(false);
useInfiniteScroll(
    scrollElement,
    async () => {
        if (!loading.dataTable && enableLoad.value) {
            loading.dataTable = true;
            let res = await getCustomers(
                "",
                customerDataTable.value.skip + 1,
                customerDataTable.value.limit
            );
            customerDataTable.value.items = [
                ...customerDataTable.value.items,
                ...res.items,
            ];
            customerDataTable.value.skip = res.skip;
            enableLoad.value = customerDataTable.value.items.length < res.total;
            loading.dataTable = false;
        }
    },
    {
        distance: 10,
    }
);

interface CustomerResponse {
    items: [];
    total: number;
    skip: number;
    limit: number;
}
const getCustomers = async (
    query: string | null,
    skip: number = 0,
    limit: number = 30
): Promise<CustomerResponse> => {
    let result: CustomerResponse = {
        items: [],
        total: 0,
        skip: 0,
        limit: 0,
    };
    try {
        const res = await API.get(`customer?search=${query}&skip=${skip}&limit=${limit}`);
        result = res.data;
    } catch (error) {
        loadError.value = true;
        console.error(error);
    }
    return result;
};

watch(
    () => modelValue.value,
    () => {
        if (modelValue.value) {
            API.get(`customer/${modelValue.value}`).then((res) => {
                customerSelected.value = res.data;
                customerSelection.value = res.data;
            });
        }
    }
);

onMounted(function () {
    if (modelValue.value) {
        API.get(`customer/${modelValue.value}`).then((res) => {
            customerSelected.value = res.data;
            customerSelection.value = res.data;
        });
    }
});
</script>

<style scoped></style>
