<template>
    <div>
        <div class="flex justify-content-between align-content-center mb-3">
            <h4 class="font-bold m-0">Báo cáo cam kết sản lượng</h4>
            <ButtonGoBack />
        </div>

        <div class="flex gap-5">
            <div class="flex align-items-center gap-3">
                <span class="font-semibold">Khách hàng:</span>
                <AutoComplete />
            </div>
            <div class="flex align-items-center gap-3">
                <span class="font-semibold">Năm cam kết:</span>
                <Calendar dateFormat="yy" view="year"></Calendar>
            </div>
            <div class="flex align-items-center gap-3">
                <span class="font-semibold">Hình thức:</span>
                <Dropdown v-model="type" :options="['Quý', 'Năm', 'Gói']"></Dropdown>
            </div>
        </div>

        <hr />
        <!-- Quý và năm -->

        <DataTable v-if="type == 'Gói'" class="card p-3" showGridlines stripedRows>
            <Column header="Ngành hàng" />
            <Column header="Thương hiệu" />
            <Column header="Tổng sản lượng cam kết" />
            <Column header="Tổng sản lượng thực tế" />
            <Column header="Chiết khấu" />
            <Column header="Quy ra hàng" />
            <template #empty>
                <div class="my-5 py-5 text-center">Không có dữ liệu để hiển thị</div>
            </template>
        </DataTable>

        <DataTable
            v-else
            class="card p-3"
            :value="value"
            rowGroupMode="rowspan"
            groupRowsBy="representative.method"
            showGridlines
            stripedRows
        >
            <ColumnGroup type="header">
                <Row>
                    <Column header="#" :rowspan="2"></Column>
                    <Column header="Ngành hàng" :rowspan="2"></Column>
                    <Column header="Thương hiệu" :rowspan="2"></Column>
                    <Column header="Qúy I" :colspan="3"></Column>
                    <Column header="Qúy II" :colspan="3"></Column>
                    <Column header="Qúy III" :colspan="3"></Column>
                    <Column header="Qúy IV" :colspan="3"></Column>
                    <Column header="CK quý" :rowspan="2"></Column>
                    <Column header="CK 9 tháng" :rowspan="2"></Column>
                    <Column header="CK năm" :rowspan="2"></Column>
                    <Column header="Quy ra hàng" :rowspan="2"></Column>
                </Row>
                <Row>
                    <template v-for="i in new Array(4)" :key="i">
                        <Column header="SL cam kết"></Column>
                        <Column header="SL thực tế"></Column>
                        <Column header="Tỷ lệ hoàn thành"></Column>
                    </template>
                </Row>
            </ColumnGroup>
            <Column v-for="i in new Array(19)" :key="i"></Column>

            <template #empty>
                <div class="my-5 py-5 text-center">Không có dữ liệu để hiển thị</div>
            </template>
        </DataTable>
    </div>

    <!-- Chi tiết từng báo cáo  -->
    <Dialog
        v-model:visible="visibleDetail"
        header="Chi tiết báo cáo cam kết sản lượng "
        modal
    >
        <DataTable
            class="card p-3"
            :value="[{}, {}, {}]"
            showGridlines
            tableStyle="min-width: 50rem"
        >
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column header="Mã đơn hàng"></Column>
        </DataTable>
    </Dialog>
</template>

<script setup>
import { ref } from "vue";

const value = [];

const type = ref("Quý");
const visibleDetail = ref(false);

const OpenDetail = () => {
    visibleDetail.value = true;
};

const goBack = () => {
    window.history.back();
};
</script>
