<template>
    <div>
        <div class="mb-3 flex justify-content-between">
            <h4 class="font-bold m-0">Báo cáo thống kê hóa đơn</h4>
            <ButtonGoBack />
        </div>
        <div class="flex gap-3 align-items-center">
            <span class="font-semibold">Thời gian:</span>
            <Calendar
                v-model="timeFilter.from"
                class="w-10rem"
                placeholder="Từ"
            ></Calendar>
            <Calendar
                v-model="timeFilter.to"
                class="w-10rem"
                placeholder="Đến"
            ></Calendar>
        </div>
        <hr />
        <DataTable class="card p-3" :value="[]" showGridlines stripedRows>
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column header="Ngày hóa đơn"></Column>
            <Column header="Số hóa đơn"></Column>
            <Column header="Giá trị hóa đơn (VND)"></Column>
            <Column header="Mã tra cứu HDDT"></Column>
            <template #empty>
                <p class="text-center py-5 my-5">
                    Không tìm thấy hóa đơn nào trong khoảng thời gian này
                </p>
            </template>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";

const timeNow = new Date();

const timeFilter = reactive({
    from: new Date(`${timeNow.getUTCFullYear()}/01/01`),
    to: new Date(),
});
</script>

<style></style>
