<template>
    <div>
        <div>
            <div class="">
                <h4 class="font-bold m-0 text-center">Báo cáo tồn kho hàng gửi</h4>
            </div>
            <div class="flex gap-3 mt-3">
                <span class="font-bold mt-2">Thời gian</span>
                <div>
                    <Calendar
                        v-model="query.FromDate"
                        class="w-10rem"
                        placeholder="Từ ngày"
                        :maxDate="new Date()"
                        :invalid="errMsg.FromDate ? true : false"
                    />
                    <small>{{ errMsg.FromDate }}</small>
                </div>
                <div>
                    <Calendar
                        v-model="query.ToDate"
                        class="w-10rem"
                        placeholder="Đến ngày"
                        :minDate="query.FromDate"
                        :maxDate="new Date()"
                        :invalid="errMsg.ToDate ? true : false"
                    />
                    <small>{{ errMsg.ToDate }}</small>
                </div>
                <!-- <div class="w-30rem">
                   
                    <CustomerSelector
                        v-model="query.CardCode"
                        selectionMode="multiple"
                        optionValue="cardCode"
                        :invalid="errMsg.CardCode ? true : false"
                    />
                    <small>{{ errMsg.CardCode }}</small>
                </div> -->
                <div>
                    <Button @click="onClickConfirm" label="Áp dụng"/>
                </div>
            </div>

            <hr />
            <div class="card p-3">
                <DataTable
                    :value="dataTable"
                    showGridlines
                    tableStyle="min-width: 124rem"
                    selectionMode="single"
                    @row-click="OpenDetail"
                    scrollable
                    scrollHeight="620px"
                    rowGroupMode="subheader"
                    groupRowsBy="cardCode"
                    :pt="{
                        rowgroupheader: {
                            class: 'surface-100',
                        },
                    }"
                >
                    <template #groupheader="slotProps">
                        <div class="flex align-items-center gap-2">
                            <span class="font-bold">{{ slotProps.data.cardCode }}</span>
                            <Divider layout="vertical"></Divider>
                            <span class="font-bold">{{ slotProps.data.cardName }}</span>
                        </div>
                    </template>
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column field="itemCode" header=" Mã hàng hóa"></Column>
                    <Column field="itemName" header=" Tên hàng hóa"></Column>
                    <Column
                        field="packagingSpecifications"
                        header=" Quy cách bao bì"
                    ></Column>
                    <Column field="category" header=" Ngành hàng"></Column>
                    <Column field="brand" header=" Thương hiệu"></Column>
                    <Column field="productType" header=" Loại hàng hóa"></Column>
                    <Column
                        field="beginQty"
                        header=" SL tồn kho đầu kỳ"
                        class="text-right"
                    >
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column field="inQty" header=" SL nhập " class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column field="outQty" header=" SL xuất " class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column
                        field="endQty"
                        header=" SL tồn kho cuối kỳ"
                        class="text-right"
                    >
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column class="hidden"></Column>
                    <ColumnGroup type="footer">
                        <Row>
                            <Column footer="Tổng cộng" :colspan="7" />
                            <Column :footer="dTfooter.beginQty" class="text-right" />
                            <Column :footer="dTfooter.inQty" class="text-right" />
                            <Column :footer="dTfooter.outQty" class="text-right" />
                            <Column :footer="dTfooter.endQty" class="text-right" />
                        </Row>
                    </ColumnGroup>
                    <template #empty>
                        <div class="py-5 my-5 text-center text-500 font-italic">
                            Không có dữ liệu để hiển thị
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>

        <!-- Chi tiết từng báo cáo  -->
        <Dialog v-model:visible="visibleDetail" header="Chi tiết báo cáo" modal>
            <div class="card p-3">
                <DataTable
                    :value="detailTable"
                    showGridlines
                    tableStyle="min-width: 50rem"
                >
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column header="Mã đơn hàng / yêu cầu lấy hàng gửi"></Column>
                    <Column header="Thời gian">
                        <template #body="{ data }">
                            <span>{{ format(data.docDate, "dd/MM/yyyy") }}</span>
                        </template>
                    </Column>
                    <Column header="Số lượng tồn đầu kỳ" class="text-right"></Column>
                    <Column field="inQty" header="Số lượng nhập" class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column field="outQty" header="Số lượng xuất" class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column header="Số lượng tồn kho cuối kỳ" class="text-right"></Column>
                    <template #empty>
                        <div class="py-5 my-5 text-center text-500 font-italic">
                            Không có dữ liệu để hiển thị
                        </div>
                    </template>
                </DataTable>
            </div>
        </Dialog>
        <Loading v-if="loading.global" />
    </div>
    <!--  Bộ lọc -->
</template>

<script setup lang="ts">
import { reactive, ref, watchEffect, computed } from "vue";
import API from "@/api/api-main";
import { format } from "date-fns";
import { Validator, ValidateOption } from "@/helpers/validate";
import uniq from "lodash/uniq";
import { useMeStore } from "@/Pinia/me";

const visibleDetail = ref(false);
const currentDate = new Date();

const dataTable = ref([]);

const errMsg = ref<any>({});
const validateOption: ValidateOption = {
    FromDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn thời gian",
            type: Date,
        },
    },
    ToDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn thời gian",
            type: Date,
        },
    },
    // CardCode: {
    //     validators: {
    //         required: true,
    //         nullMessage: "Vui lòng chọn khách hàng",
    //     },
    // },
};

const meStore = useMeStore();

const query = reactive({
    FromDate: new Date(`${currentDate.getFullYear()}-01-01`),
    ToDate: currentDate,
    CardCode: "",
});

const onClickConfirm = () => {
    const vResult = Validator(query, validateOption);
    errMsg.value = {};
    if (!vResult.result) {
        errMsg.value = vResult.errors;
        return;
    }
    loading.global = true;
    query.CardCode = meStore.computedMe?.user?.bpInfo?.cardCode;
    fetchData(toQueryString());
};

const toQueryString = (): string => {
    return Object.entries(query)
        .map(([key, value]) =>
            value
                ? `${key}=${
                      isDate(value)
                          ? format(value as Date, "yyyy-MM-dd")
                          : `,${Array(value).join(",")},`
                  }`
                : null
        )
        .filter((item) => item)
        .join("&");
};

const detailTable = ref<any[]>([]);
const dTfooter = reactive({
    beginQty: "",
    inQty: "",
    outQty: "",
    endQty: "",
});

const sumColumn = (array: any[], field: string): string => {
    let result = array.reduce((sum: number, item: any) => {
        return sum + item[field];
    }, 0);
    return Intl.NumberFormat().format(result);
};

const OpenDetail = (event: any) => {

    detailTable.value = event.data.inventoryDetail;
    visibleDetail.value = true;
};

const loading = reactive({
    global: false,
});
const fetchData = (query: string) => {
    API.get(`Report/InventoryReport?${query}`)
        .then((res) => {
            dataTable.value = [];
            Object.keys(dTfooter).forEach((key) => {
                dTfooter[key as keyof typeof dTfooter] = "0";
            });
            if (res.data) {
                dataTable.value = groupArray(res.data, "cardCode");
                Object.keys(dTfooter).forEach((key) => {
                    dTfooter[key as keyof typeof dTfooter] = sumColumn(res.data, key);
                });
            }
        })
        .catch()
        .finally(() => {
            loading.global = false;
        });
};

const isDate = (value: any): boolean => {
    return value instanceof Date;
};

function groupArray<T>(array: T[], groupBy: string): T[] {
    // Add your logic here
    const result: T[] = [];
    const fields = uniq(array.map((item) => item[groupBy as keyof T]));
    fields.forEach((field) => {
        const group = array.filter((item) => item[groupBy as keyof T] === field);
        result.push(...group);
    });
    return result;
}

// watchEffect(() => {
//     if (query.FromDate && query.ToDate && query.CardCode?.length) {
//         loading.global = true;
//         fetchData(toQueryString());
//     }
// });
</script>
<style scoped lang="css">
small {
    color: var(--red-500);
}
</style>
