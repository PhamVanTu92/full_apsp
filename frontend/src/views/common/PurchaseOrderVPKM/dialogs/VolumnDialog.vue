<template>
    <div>
        <Dialog
            v-model:visible="visible"
            style="width: 70vw; max-width: 100rem !important"
            modal
            header="Thông tin sản lượng"
            @hide="initialComponent"
        >
            <DataTable :value="poStore.model.itemDetail">
                <Column header="#">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column>
                <Column field="itemName" header="Tên sản phẩm"></Column>
                <Column field="uomName" header="Đơn vị tính"></Column>
                <Column field="quantity" header="Số lượng" class="text-right"></Column>
                <Column field="_volumn" header="Sản lượng (lít)" class="text-right">
                    <template #body="{ data, field }">
                        {{ fnum(data._volumn) }}
                    </template>
                </Column>
                <Column header="Sản lượng được tính thưởng (lít)" class="text-right">
                    <template #body="{ data, field }">
                        {{ fnum(data._volumn * data.quantity) }}
                    </template>
                </Column>
                <Column header="Được tính chiết khấu sản lượng"></Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column footer="Tổng" :colspan="4"></Column>
                        <Column :footer="volumns" class="text-right"></Column>
                        <Column :footer="reachVolumns" class="text-right"></Column>
                        <Column footer="-" class="text-right"></Column>
                    </Row>
                </ColumnGroup>
            </DataTable>
            <template #footer>
                <Button
                    @click="visible = false"
                    label="Đóng"
                    severity="secondary"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { usePoStore } from "../store/purchaseStore.store";
import { fnum } from "../script";

const visible = ref(false);
const poStore = usePoStore();

const volumns = computed(() => {
    return poStore.model.itemDetail
        .reduce((sum, item) => (sum += item._volumn), 0)
        .toLocaleString(undefined, { style: "decimal" });
});
const reachVolumns = computed(() => {
    return poStore.model.itemDetail
        .reduce((sum, item) => (sum += item._volumn * item.quantity), 0)
        .toLocaleString(undefined, { style: "decimal" });
});

const open = () => {
    visible.value = true;
};

defineExpose({
    open,
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
