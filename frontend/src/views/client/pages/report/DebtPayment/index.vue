<template>
    <div>
        <div class="mb-3 flex justify-content-center">
            <h4 class="font-bold mb-0">Báo cáo công nợ phải trả</h4>
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-semibold">Thời gian:</span>
            <span>{{ format(timeFilter.to, "dd/MM/yyyy") }}</span>
        </div>
        <hr />
        <div class="flex justify-content-end mb-4">
            <DataTable showGridlines :value="paymentMethod" class="card p-3">
                <Column field="type" header="Hình thức thanh toán"></Column>
                <Column field="limit" header="Hạn mức công nợ"></Column>
                <Column field="actual" header="Công nợ thực tế (VND)"></Column>
                <Column field="over" header="Số tiền vượt hạn mức (VND)"></Column>
                <Column field="dueDate" header="Thời hạn thanh toán "></Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column footer="Tổng:" />
                        <Column footer="- VND" />
                        <Column footer="- VND" />
                        <Column footer="- VND" />
                        <Column />
                    </Row>
                </ColumnGroup>
            </DataTable>
        </div>
        <DataTable class="card p-3 mb-5" showGridlines stripedRows :value="dataTable">
            <ColumnGroup type="header">
                <Row>
                    <Column header="#" :rowspan="2"></Column>
                    <Column header="Mã hóa đơn" :rowspan="2"></Column>
                    <Column header="Ngày chứng từ" :rowspan="2"></Column>
                    <Column header="Số chứng từ" :rowspan="2"></Column>
                    <Column header="Hình thức thanh toán" :colspan="2"></Column>
                    <Column header="Số tiền vượt hạn mức" :rowspan="2"></Column>
                    <Column header="Trạng thái" :rowspan="2"></Column>
                    <Column header="Ngày đến hạn" :rowspan="2"></Column>
                    <Column header="Số ngày quá hạn (Ngày)" :rowspan="2"></Column>
                </Row>
                <Row>
                    <Column header="Nợ tín chấp"></Column>
                    <Column header="Nợ bảo lãnh"></Column>
                </Row>
            </ColumnGroup>
            <Column field="">
                <template #body="sp">
                    {{ sp.index + 1 }}
                </template>
            </Column>
            <Column field="docCode" header="Mã đơn hàng"></Column>
            <Column field="invoiceCode" header="Số hóa đơn"></Column>
            <Column field="docDate" header="Ngày chứng từ">
                <template #body="{ data, field }">
                    {{ format(new Date(data[field]), "dd/MM/yyyy") }}
                </template>
            </Column>
            <Column field="docTotal" header="Giá trị gốc" bodyClass="text-right">
                <template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template>
            </Column>
            <Column field="docTotal" header="Giá trị gốc" bodyClass="text-right">
                <template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template>
            </Column>
            <Column field="" header="Giá trị còn lại" bodyClass="text-right">
                <template #body="{ data, field }">
                    {{ fnum(data["docTotal"] - data["paidToDate"]) }}
                </template>
            </Column>
            <Column field="types" header="Hình thức thanh toán"></Column>
            <Column field="docDueDate" header="Ngày đến hạn">
                <template #body="{ data, field }">
                    {{ format(new Date(data[field]), "dd/MM/yyyy") }}
                </template>
            </Column>
            <Column
                field="paidToDate"
                header="Số ngày quá hạn (Ngày)"
                bodyClass="text-right"
            >
                <template #body="{ data, field }">
                    {{ distanceDate(data["docDueDate"]) }}
                </template>
            </Column>

            <template #empty>
                <div class="py-5 my-5 text-center">Không có dữ liệu</div>
            </template>

            <ColumnGroup type="footer">
                <Row>
                    <Column footer="Tổng:" :colspan="4" footerStyle="text-align:right" />
                    <Column :footer="'- VND'" footerStyle="text-align:right" />
                    <Column :footer="'- VND'" footerStyle="text-align:right" />
                    <Column :footer="'- VND'" footerStyle="text-align:right" />
                    <Column footerStyle="text-align:right" />
                    <Column footerStyle="text-align:right" />
                    <Column footerStyle="text-align:right" />
                </Row>
            </ColumnGroup>
        </DataTable>
        <Loading v-if="loading"></Loading>
        <!-- <div class="p-8 m-8 text-center text-500 text-xl">PENDING!</div> -->
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { format } from "date-fns";
import API from "@/api/api-main";
import { isArray } from "lodash";
import { fnum } from "@/views/common/PurchaseOrder/script";

const dataTable = ref([]);

const timeNow = new Date();
const timeFilter = reactive({
    to: timeNow,
});

const paymentMethod = ref([
    {
        type: "Nợ tín chấp",
        limit: "0",
        actual: "0",
        over: "0",
        dueDate: "-",
    },
    {
        type: "Nợ bảo lãnh",
        limit: "0",
        actual: "0",
        over: "0",
        dueDate: "-",
    },
]);

const distanceDate = (date1: string, date2 = new Date()) => {
    const d1 = new Date(date1);
    const d2 = new Date(date2);
    const diffTime = d2.getTime() - d1.getTime();
    if (diffTime > 0) return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    else return 0;
};

const loading = ref(false);
onMounted(function () {
    const cardId = JSON.parse(localStorage.getItem("user") || "{}")?.appUser?.cardId;
    if (cardId) {
        loading.value = true;
        API.get(
            `Report/debtReport?ToDate=${format(
                timeFilter.to,
                "yyyy-MM-dd"
            )}&CardCode=${cardId}`
        )
            .then((res) => {
                if (isArray(res.data) && res.data?.length > 0) {
                    const data = res.data[0];
                    paymentMethod.value[0].limit = data["totalCredit"];
                    paymentMethod.value[0].actual = data["credit"];
                    paymentMethod.value[0].over = data["overCredit"];
                    paymentMethod.value[0].dueDate = data["timeCredit"];
                    paymentMethod.value[1].limit = data["totalGuarantee"];
                    paymentMethod.value[1].actual = data["guarantee"];
                    paymentMethod.value[1].over = data["overGuarantee"];
                    paymentMethod.value[1].dueDate = data["timeGuarantee"];

                    dataTable.value = data["bccnDetail"];
                }
            })
            .catch((error) => {})
            .finally(() => {
                loading.value = false;
            });
    }
});
</script>

<style scoped></style>
