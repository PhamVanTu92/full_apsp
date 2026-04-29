<template>
    <div>
        <Dialog
            v-model:visible="visible"
            header="Danh sách khách hàng"
            modal
            style="max-width: 90vw; width: 80rem"
            @hide="onHide"
        >
            <!--  data.customers -->
            <DataTable
                :value="
                    data.customers.filter((item) => !props.exceptionIds.includes(item.id))
                "
                id="customerTable"
                v-model:selection="data.selectedCustomers"
                lazy
                show-gridlines
                striped-rows
                selection-mode="multiple"
                scrollable
                scroll-height="60vh"
                style="height: 70vh"
            >
                <template #header>
                    <div class="flex justify-content-end">
                        <InputText
                            v-model="query.search"
                            placeholder="Tìm kiếm..."
                            @input="onSearch"
                        ></InputText>
                    </div>
                </template>
                <Column selection-mode="multiple"></Column>
                <Column field="cardCode" header="Mã khách hàng" class="w-12rem"></Column>
                <Column field="cardName" header="Tên khách hàng"></Column>
                <Column field="email" header="Email"></Column>
                <template #empty>
                    <div v-if="!loading" class="py-5 my-5 text-center">
                        Không có dữ liệu
                    </div>
                    <div v-else class="py-5 my-5 text-center"></div>
                </template>
                <template #footer>
                    <div class="font-normal text-center font-bold">
                        {{ loading ? "Đang tải..." : "&nbsp;" }}
                    </div>
                </template>
            </DataTable>
            <template #footer>
                <div class="flex justify-content-between align-items-center w-full">
                    <div>
                        <div class="flex gap-2" v-if="data.selectedCustomers.length">
                            <div class="flex gap-2 align-items-center">
                                Đã chọn
                                <span class="font-semibold">{{
                                    data.selectedCustomers.length
                                }}</span>
                                khách hàng
                            </div>
                            <Button
                                label="Bỏ chọn"
                                @click="data.selectedCustomers = []"
                                icon="pi pi-times"
                                text
                                size="small"
                                severity="danger"
                                class="px-1"
                            />
                        </div>
                    </div>
                    <Button
                        @click="onClickSelect"
                        label="Chọn"
                        :disabled="data.selectedCustomers.length < 1"
                    />
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import API from "@/api/api-main";
import { AxiosResponse } from "axios";
import { useInfiniteScroll } from "@vueuse/core";
import { nextTick } from "vue";
import debounce from "lodash/debounce";
import cloneDeep from "lodash/cloneDeep";

const props = defineProps({
    exceptionIds: {
        type: Array<Number>,
        default: [],
        required: true
    }
})

const visible = ref(false);
const loading = ref(false);
const query = {
    search: "",
    limit: 50,
    skip: 0,
    // filter: "(portalRegistrationStatus=N)",
};

const data = reactive({
    selectedCustomers: [] as any[],
    customers: [] as any[],
    total: -1,
});

const onSearch = debounce(() => {
    query.skip = 0;
    data.customers = [];
    loading.value = false;
    stopFlag = false;
    fetchCustomerUnregisted();
    query.skip++;
}, 500);

const dataTableRef = ref<HTMLElement>();
useInfiniteScroll(
    dataTableRef,
    () => {
        if (loading.value) return;
        if (stopFlag) return;
        fetchCustomerUnregisted();
        query.skip++;
    },
    { distance: 200 }
);

const getQuery = (): string => {
    const urlSearchParam = new URLSearchParams(
        Object.fromEntries(
            Object.entries(query).filter(item => item[1] != null).map(([key, value]) => [key, String(value)])
        )
    );
    return urlSearchParam.toString();
};

const open = () => {
    visible.value = true;
    nextTick(() => {
        dataTableRef.value = document.querySelector(
            "#customerTable > div.p-datatable-wrapper"
        ) as HTMLElement;
    });
};

const onHide = () => {
    data.selectedCustomers = [];
    data.customers = [];
    data.total = -1;
    query.skip = 0;
    query.search = '';
    stopFlag = false;
};

const emits = defineEmits(["item-select"]);

const onClickSelect = () => {
    emits("item-select", cloneDeep(data.selectedCustomers));
    visible.value = false;
};

var stopFlag = false;
const fetchCustomerUnregisted = (): void => {
    loading.value = true;
    API.get(`customer?${getQuery()}`)
        .then((res: AxiosResponse) => {
            data.total = res.data?.total;
            let resdata = res.data?.items as Array<any>;
            if(resdata?.length){
                data.customers = [...data.customers, ...resdata];
            }
            if (data.customers.length >= data.total) {
                stopFlag = true;
                return;
            }
        })
        .catch((error) => {
            stopFlag = true;
        })
        .finally(() => {
            loading.value = false;
        });
};
defineExpose({ open });

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
