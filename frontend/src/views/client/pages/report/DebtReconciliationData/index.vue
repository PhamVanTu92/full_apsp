<template>
    <div>
        <div class="mb-3 flex justify-content-center">
            <h4 class="font-bold mb-0">Danh sách biên bản đối chiếu công nợ</h4>
        </div>
        <hr />
        <DataTable class="card p-3" :value="[]" showGridlines stripedRows>
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column header="Tên biên bản"></Column>
            <Column header="Ngàygửi"></Column>
            <Column header="Ngày xác nhận"></Column>
            <Column header="Tệp đính kèm"> </Column>
            <Column header="Tệp khách hàng gửi lại"> </Column>
            <Column header="Ghi chú/ Lý do "> </Column>
            <Column header="Trạng thái"> </Column>
            <template #header>
                <div class="flex">
                    <IconField>
                        <InputIcon class="pi pi-search"> </InputIcon>
                        <InputText placeholder="Tìm kiếm..."></InputText>
                    </IconField>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-center">Không có dữ liệu để hiển thị</div>
            </template>
        </DataTable>
    </div>
</template>
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
const timeNow = new Date();
const timeFilter = reactive({
    from: new Date(`${timeNow.getFullYear()}/01/01`),
    to: timeNow,
});

onMounted(function () {});
</script>

<style scoped></style>
