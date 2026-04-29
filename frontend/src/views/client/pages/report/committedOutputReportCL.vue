<template>
    <div>
        <div class="flex justify-content-center align-content-center mb-3">
            <h4 class="font-bold m-0">Báo cáo cam kết sản lượng</h4>
            <!-- <Button label="Bộ lọc" icon="pi pi-filter"/> -->
            <div v-if="0" class="flex gap-2">
                <Button
                    label="In báo cáo"
                    outlined
                    icon="pi pi-print"
                    severity="warning"
                />
                <Button
                    label="Xuất excel"
                    outlined
                    icon="pi pi-file-export"
                    severity="info"
                />
                <Button
                    label="Bộ lọc"
                    icon="pi pi-filter"
                    @click="visibleFilter = true"
                />
            </div>
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-semibold">Thời gian:</span>
            <Calendar
                v-model="timeFilter.from"
                placeholder="Từ"
                class="w-10rem"
            ></Calendar>
            <Calendar
                v-model="timeFilter.to"
                placeholder="Đến"
                class="w-10rem"
            ></Calendar>
        </div>

        <hr />
        <div class="card">
            <DataTable
                :value="value"
                rowGroupMode="rowspan"
                groupRowsBy="representative.method"
                showGridlines
                tableStyle="min-width: 50rem"
            >
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column
                    field="representative.method"
                    header="Hình thức"
                    style="width: 6rem"
                >
                    <template #body="{ data }">
                        <div class="text-center">
                            {{ data.representative.method }}
                        </div>
                    </template>
                </Column>
                <Column
                    field="industry"
                    header="Ngành hàng"
                    style="width: 10rem"
                ></Column>
                <Column header="Thương hiệu" style="width: 10rem"></Column>
                <Column header=" Loại sản phẩm" style="width: 10rem"></Column>
                <Column header=" Sản lượng cam kết" style="width: 10rem"></Column>
                <Column header=" Sản lượng thực tế" style="width: 10rem"></Column>
                <Column header=" Tỷ lệ hoàn thành" style="width: 10rem"></Column>
                <Column style="width: 5rem">
                    <template #body="{ data }">
                        <Button icon="pi pi-eye" @click="OpenDetail" text/>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <!-- Chi tiết từng báo cáo  -->
    <Dialog
        v-model:visible="visibleDetail"
        header="Chi tiết báo cáo cam kết sản lượng "
        modal
    >
        <div class="card">
            <DataTable :value="[{}, {}, {}]" showGridlines tableStyle="min-width: 50rem">
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column header=" Mã đơn hàng" style="width: 9rem"></Column>
                <Column header=" Ngành hàng" style="width: 9rem"></Column>
                <Column header=" Thương hiệu" style="width: 10rem"></Column>
                <Column header=" Loại sản phẩm" style="width: 10rem"></Column>
                <Column header=" Tổng sản lượng " style="width: 10rem"></Column>
                <Column header=" Sản lượng được tính " style="width: 12rem"></Column>
                <Column header=" Sản lượng tích lũy " style="width: 11rem"></Column>
            </DataTable>
        </div>
    </Dialog>

    <Dialog
        v-if="0"
        v-model:visible="visibleFilter"
        header="Bộ lọc"
        style="width: 35%"
        modal
    >
        <div class="flex justify-content-between align-content-evenly card">
            <div class="grid w-full">
                <div class="col-6 flex flex-column gap-2">
                    <label class="font-bold" for=""> Hình thức</label>
                    <Calendar
                        v-model="dates"
                        selectionMode="range"
                        :manualInput="false"
                        placeholder=" Hình thức "
                    />
                </div>

                <div class="col-6 flex flex-column gap-2">
                    <label class="font-bold" for=""> Ngành hàng </label>
                    <Dropdown
                        optionLabel="name"
                        placeholder=" Ngành hàng"
                        class="w-full md:w-14rem"
                    />
                </div>
                <div class="col-6 flex flex-column gap-2">
                    <label class="font-bold" for=""> Thương hiệu </label>
                    <Dropdown
                        optionLabel="name"
                        placeholder="Thương hiệu"
                        class="w-full md:w-14rem"
                    />
                </div>
                <div class="col-6 flex flex-column gap-2">
                    <label class="font-bold" for=""> Loại sản phẩm </label>
                    <Dropdown
                        optionLabel="name"
                        placeholder="  Loại sản phẩm"
                        class="w-full md:w-14rem"
                    />
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button
                    type="button"
                    label="Bỏ qua"
                    severity="secondary"
                    @click="visibleFilter = false"
                />
                <Button
                    type="button"
                    label="Xác nhận"
                    @click="visibleFilter = false"
                />
            </div>
        </template>
    </Dialog>
</template>

<script setup>
import { ref, reactive } from "vue";
const timeNow = new Date();
const timeFilter = reactive({
    from: new Date(`${timeNow.getFullYear()}/01/01`),
    to: timeNow,
});
const value = [
    {
        industry: "HDEO",
        brand: "AP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Quý",
        },
    },
    {
        industry: "HDEO",
        brand: "AP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Quý",
        },
    },
    {
        industry: "HDEO",
        brand: "AP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Quý",
        },
    },
    {
        industry: "HDEO",
        brand: "SP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Quý",
        },
    },
    {
        industry: "HDEO",
        brand: "AP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Gói",
        },
    },
    {
        industry: "HDEO",
        brand: "AP",
        productType: "Động cơ",
        commitedProduct: 10000,
        actualProduct: 2000,
        donePercent: 20,
        representative: {
            method: "Năm",
        },
    },
];

const visibleFilter = ref(true);
const visibleDetail = ref(false);

const OpenDetail = () => {
    visibleDetail.value = true;
};

const goBack = () => {
    window.history.back();
};
</script>
