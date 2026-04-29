<template>
    <div>
        <Dialog
            v-model:visible="visible"
            modal
            style="max-width: 100rem !important; width: 90wv"
            :header="t('client.commitment_content')"
            :contentStyle="{ minHeight: '300px' }"
            @hide="onHide"
        >
            <!-- <div>{{ poStore.rawCommitted?.committedLine }}</div> -->
            <div
                v-for="(line, i) in poStore.rawCommitted?.committedLine || []"
                :key="i"
                class="card p-3"
            >
                <DataTable
                    v-model:expandedRows="expandeRows[i]"
                    :value="line.committedLineSub"
                    resizableColumns
                    columnResizeMode="fit"
                    scrollable
                    scrollHeight="flex"
                >
                    <template #header>
                        <span class="mr-2">{{t('client.application_method')}}</span>
                        <span class="font-bold">{{
                            line.committedType == "Q" ? t('client.quarter') :  t('Custom.year')
                        }}</span>
                    </template>
                    <Column
                        v-if="line.committedLineSub[i]?.committedLineSubSub?.length"
                        expander
                    ></Column>
                    <Column field="industryName" :header="t('client.industry')"></Column>
                    <Column field="brand" :header="t('client.brand')">
                        <template #body="{ data, field }">
                            <div class="flex gap-2">
                                <span>
                                    {{ getLabelArray(data[field], "name") }}
                                </span>

                                <Button
                                    v-if="data[field]?.length > 1"
                                    text
                                    size="small"
                                    :label="t('Custom.seeMore')"
                                    class="p-0"
                                    @click="onClickShowMoreData($event, data[field])"
                                />
                            </div>
                        </template>
                    </Column>
                    <Column field="itemTypes" :header="t('client.product_type')">
                        <template #body="{ data, field }">
                            <div class="flex gap-2">
                                <span>
                                    {{ getLabelArray(data[field], "name") }}
                                </span>

                                <Button
                                    v-if="data[field]?.length > 1"
                                    text
                                    size="small"
                                    :label="t('Custom.seeMore')"
                                    class="p-0"
                                    @click="onClickShowMoreData($event, data[field])"
                                />
                            </div>
                        </template>
                    </Column>
                    <Column
                        :field="col.field"
                        :header="col.header"
                        :key="i"
                        v-for="(col, i) in getColumns(line)"
                    >
                        <template #body="{ data, field }">
                            <div class="flex justify-content-end">
                                <span>{{ fnum(data[field]) }}{{ col.suffix }}</span>
                                <span v-if="col.subField ? data[col.subField] : false"
                                    >Quy ra hàng</span
                                >
                            </div>
                        </template>
                    </Column>
                    <template #expansion="slotProps">
                        <DataTable :value="slotProps.data?.committedLineSubSub">
                            <Column header="#" class="w-3rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column header="Sản lượng vượt" class="w-12rem">
                                <template #body="{ data }">
                                    {{ fnum(data["outPut"]) }}
                                </template>
                            </Column>
                            <Column :header="t('Custom.discount')">
                                <template #body="{ data }">
                                    <div>
                                        <span class="mr-3">
                                            {{ fnum(data["discount"]) }}%
                                        </span>
                                        <span v-if="data['isConvert']">{{t('Custom.queueUp')}}</span>
                                    </div>
                                </template>
                            </Column>
                        </DataTable>
                    </template>
                </DataTable>
            </div>
            <template #footer>
                <Button
                    @click="visible = false"
                    :label="t('client.cancel')"
                    severity="secondary"
                />
            </template>
        </Dialog>
        <OverlayPanel ref="opShowMoreRef">
            <Listbox
                :options="moreData"
                optionLabel="name"
                class="w-full md:w-20rem"
                listStyle="max-height:250px"
            />
        </OverlayPanel>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { usePoStore } from "../store/purchaseStore.store";
import { fnum } from "../script";
import { CommittedLine } from "../types/committed.type";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const poStore = usePoStore();
const visible = ref(false);
const expandeRows = ref([]);

const opShowMoreRef = ref();
const moreData = ref<any[]>([]);

const open = () => {
    visible.value = true;
    expandeRows.value = [];
};

defineExpose({
    open,
});

const onHide = () => {
    expandeRows.value = [];
    visible.value = false;
};

const onClickShowMoreData = (event: Event, data: any[]) => {
    moreData.value = data;
    opShowMoreRef.value?.toggle(event);
};

const getLabelArray = (data: any[], field: string) => {
    if (data.length > 1) {
        return `${data[0]?.[field]}, ...(+${data[0]?.[field].length - 1})`;
    } else if (data.length < 1) {
        return data[0]?.[field];
    }
    return "";
};

type Column = {
    field: string;
    suffix?: string;
    header: string;
    subField?: string;
};
const getColumns = (line: CommittedLine) => {
    const cols: Column[] = [];
    if (line.committedType == "Q") {
        for (let i = 1; i <= 4; i++) {
            cols.push({
                field: "quarter" + i,
                header: `${t('client.quarter')} ` + i,
            });
        }
    } else if (line.committedType == "Y") {
        for (let i = 1; i <= 12; i++) {
            cols.push({
                field: "month" + i,
                header: t('client.months_duration') + i,
            });
        }
    }
    cols.push({
        field: "total",
        header: t('Custom.discount') + line.committedType == "Q" ? t('client.quarter') : t('Custom.year'),
        subField: "isConvert",
    });
    for (let item of [
        [3, "threeMonthDiscount", "isCvThreeMonth"],
        [6, "sixMonthDiscount", "isCvSixMonth"],
        [9, "nineMonthDiscount", "isCvNineMonth"],
    ]) {
        cols.push({
            field: item[1] as string,
            header: `${t('Custom.discount')} ${item[0]} ${t('client.months_duration')}`,
            subField: item[2] as string,
            suffix: "%",
        });
    }
    cols.push({
        field: "discountYear",
        header: t('client.annual_discount'),
        subField: "isCvYear",
        suffix: "%",
    });
    return cols;
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
