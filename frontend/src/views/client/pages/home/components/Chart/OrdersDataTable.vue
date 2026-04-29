<template>
    <DataTable showGridlines="" stripedRows="" :value="[]">
        <Column header="Mã Đơn Hàng" field=""></Column>
        <Column header="Hình Thức Công Nợ" field=""></Column>
        <Column header="Số Tiền (VNĐ)" field=""></Column>
        <Column header="Ngày Đến Hạn" field=""></Column>
        <Column header="Trạng Thái" field=""></Column>
        <template #empty>
            <div class="my-5 py-5 text-center">Không có dữ liệu để hiển thị</div>
        </template>
    </DataTable>
</template>

<script setup></script>

<style></style>
