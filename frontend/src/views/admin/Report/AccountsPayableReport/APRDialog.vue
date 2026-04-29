<template>
    <div>
        <Dialog v-model:visible="visible" modal header="Chi tiết công nợ">
            <div>
                <div class="card p-3">
                    <div class="mb-2 flex">
                        <div class="font-semibold w-10rem">Mã khách hàng:</div>
                        <router-link class="font-bold text-primary" to="#">{{
                            data.cardCode
                        }}</router-link>
                    </div>
                    <div class="flex">
                        <div class="font-semibold w-10rem">Khách hàng:</div>
                        <span>{{ data.cardName }}</span>
                    </div>
                </div>
                <DataTable
                    :value="data.tableValue"
                    class="card p-3"
                    showGridlines
                    stripedRows
                    selection-mode="single"
                    @row-click="onRowClick"
                >
                    <Column header="#">
                        <template #body="{ index }">
                            <span>{{ index + 1 }}</span>
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
                        <div class="py-5 my-5 text-center">
                            Không có dữ liệu để hiển trị
                        </div>
                    </template>
                </DataTable>
            </div>
            <template #footer>
                <Button @click="visible = false" severity="secondary">Đóng</Button>
            </template>
        </Dialog>
        <Dialog
            v-model:visible="detailDlg.visible"
            :header="detailDlg.header"
            style="width: 50rem"
            modal
        >
            <DataTable :value="detailDlg.dtValue" show-gridlines striped-rows>
                <Column field="paymentNumber" header="Mã phiếu thu"></Column>
                <Column field="docDate" header="Ngày chứng từ">
                    <template #body="{ data, field }">
                        {{ fDate(data[field]) }}
                    </template>
                </Column>
                <Column field="amountPaid" header="Giá trị (VND)">
                    <template #body="{ data, field }">
                        {{ Intl.NumberFormat().format(data[field]) }}
                    </template>
                </Column>
                <template #empty>
                    <div class="py-5 my-5 text-center">Không có dữ liệu</div>
                </template>
            </DataTable>
            <template #footer>
                <Button
                    label="Đóng"
                    severity="secondary"
                    @click="detailDlg.visible = false"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { format } from "date-fns";
import { fDate } from "../../../../utils/format";

type Prop = {
    tableValue: never[];
    cardCode: string;
    cardName: string;
};

const data = reactive<Prop>({
    tableValue: [],
    cardCode: "",
    cardName: "",
});

const visible = ref(false);
const visibleDetail = ref(false);
const detailDlg = reactive({
    visible: false,
    header: "",
    dtValue: [],
});

function onRowClick(event: any) {
    detailDlg.header = "Chi tiết hóa đơn " + event.data.docCode;

    detailDlg.dtValue = event.data?.bccnInComing || [];
    detailDlg.visible = true;
}

function openDialog(prop: Prop) {
    data.tableValue = prop.tableValue;
    data.cardCode = prop.cardCode;
    data.cardName = prop.cardName;
    visible.value = true;
}

const fnum = (num: number) => {
    if (!num) return "";
    return Intl.NumberFormat().format(num);
};

const distanceDate = (date1: string, date2 = new Date()) => {
    const d1 = new Date(date1);
    const d2 = new Date(date2);
    const diffTime = d2.getTime() - d1.getTime();
    if (diffTime > 0) return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    else return 0;
};

defineExpose({
    open: openDialog,
});

onMounted(function () {});
</script>

<style scoped></style>
