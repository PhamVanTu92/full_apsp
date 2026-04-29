<template>
    <div class="">
        <div class="flex justify-content-between align-items-center">
            <h4 class="font-bold">Báo cáo kế hoạch nhập hàng</h4>
            <ButtonGoBack />
        </div>
        <div class="flex align-items-center gap-5 mt-3">
            <div class="flex gap-3 align-items-center mt3">
                <span>Năm: </span>
                <!-- <span>{{ '-' }}</span> -->
                <Calendar
                    v-model="selectY"
                    view="year"
                    placeholder="Chọn năm"
                    dateFormat="yy"
                />
            </div>
            <div class="flex gap-3 align-items-center">
                <span>Mã kế hoạch: </span>
                <!-- <span>{{ '-' }}</span> -->
                <Dropdown
                    v-model="selectC"
                    placeholder="Chọn kế hoạch"
                    class="w-full md:w-14rem"
                />
            </div>
            <hr />
        </div>
        <div class="card mt-4">
            <DataTable :value="[]" showGridlines>
                <ColumnGroup type="header">
                    <Row>
                        <Column header="Mã hàng hóa" :rowspan="2" class="text-center" />
                        <Column header="Tên hàng hóa" :rowspan="2" class="text-center">
                        </Column>
                        <Column
                            header="Quy cách bao bì"
                            :rowspan="2"
                            class="text-center"
                        ></Column>
                        <Column header="Thương hiệu" :rowspan="2" class="text-center" />
                        <Column header="Ngành hàng" :rowspan="2" class="text-center">
                        </Column>
                        <Column
                            header="Loại hàng hóa"
                            :rowspan="2"
                            class="text-center"
                        ></Column>
                        <Column
                            v-for="i in 3"
                            :key="i"
                            :colspan="3"
                            :header="`Tháng`"
                            class="text-center"
                        ></Column>
                    </Row>
                    <Row>
                        <template v-for="i in 3" :key="i">
                            <Column class="text-center" header="SL kế hoạch"></Column>
                            <Column class="text-center" header="SL thực tế"></Column>
                            <Column
                                class="text-center bg-gray-100"
                                header="Tỷ lệ chính xác"
                            ></Column>
                        </template>
                    </Row>
                </ColumnGroup>
                <Column header="Mã hàng hóa" field="" class="text-center"></Column>
                <Column header="Tên hàng hóa" field="" class="text-center"></Column>
                <Column header="Quy cách bao bì" field="" class="text-center"></Column>
                <Column header="Thương hiệu" field="" class="text-center"></Column>
                <Column header="Ngành hàng" field="" class="text-center"></Column>
                <Column header="Loại hàng hóa" field="" class="text-center"></Column>
                <template v-for="i in 3" :key="i">
                    <Column header="SL kế hoạch" class="text-center">
                        <template></template>
                    </Column>
                    <Column header="SL thực tế" class="text-center">
                        <template></template>
                    </Column>
                    <Column header="Tỷ lệ chính xác" class="bg-gray-100 text-center">
                        <template></template>
                    </Column>
                </template>

                <template #empty>
                    <p class="py-5 my-5 text-center text-muted">
                        Không có dữ liệu để hiển thị
                    </p>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<script setup></script>

<style></style>
