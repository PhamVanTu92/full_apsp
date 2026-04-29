<template>
    <div>
        <div class="flex align-items-center justify-content-between mb-4">
            <h4 class="font-bold m-0">Chi tiết yêu cầu lấy hàng gửi</h4>
            <ButtonGoBack />
        </div>

        <div class="card border-noround mb-5">
            <div class="grid">
                <div class="col-6">
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Mã đơn hàng:</div>
                        <div class="col">ds</div>
                    </div>
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Mã khách hàng:</div>
                        <div class="col">ds</div>
                    </div>
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Tên khách hàng:</div>
                        <div class="col">ds</div>
                    </div>
                </div>
                <div class="col-4">
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Thời gian lấy hàng:</div>
                        <div class="col">ds</div>
                    </div>
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Thời gian tạo:</div>
                        <div class="col">ds</div>
                    </div>
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Người tạo:</div>
                        <div class="col">ds</div>
                    </div>
                    <div class="grid m-0">
                        <div class="col-3 font-semibold">Trạng thái:</div>
                        <div class="col">ds</div>
                    </div>
                </div>
                <div class="col-2">
                    <KiemTraCongNo> </KiemTraCongNo>
                </div>
            </div>
            <DataTable :value="Array(10)" showGridlines stripedRows class="mb-3">
                <Column header="#" class="w-3rem text-right">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column>
                <Column header="Mã sản phẩm"></Column>
                <Column header="Tên hàng hóa"></Column>
                <Column header="Đơn vị tính"></Column>
                <Column header="Số lượng"></Column>
            </DataTable>
            <div class="flex flex-column">
                <label for="" class="mb-2 font-semibold">Ghi chú</label>
                <Textarea class="w-full" :rows="3" readonly></Textarea>
            </div>
            <hr />
            <div class="flex justify-content-end gap-2">
                <Button label="Xác nhận"/>
                <Button label="Xác nhận"/>
                <Button label="Xác nhận"/>
            </div>
        </div>
        <DocumentAttach
            v-model:data="documents"
            header="Tài liệu đính kèm"
            showGridlines
            class="mb-5"
        ></DocumentAttach>
        <div class="card border-noround p-0 mb-5">
            <div class="p-3 font-bold text-lg border-bottom-1 border-200">
                <div class="my-2">Thông tin yêu cầu</div>
            </div>
            <div class="p-3">dsadsa</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";

const documents = ref([]);

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.col-3,
.col {
    padding: 0;
    margin: 0 0 1rem 0;
}
.col-3 {
    min-width: 150px;
}
</style>
