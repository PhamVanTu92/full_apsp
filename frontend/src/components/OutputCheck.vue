<template>
    <!-- {{ props.customer?.id }} |{{ loading }} -->
    <div v-if="props.customer?.id && !loading" class="flex flex-column gap-3">
        <div class="card p-0 border-green-700">
            <div
                v-for="(line, i) in currentCommitedData"
                :key="i"
                class="line-data p-4"
                :class="{
                    'border-bottom-1 border-green-700':
                        i < currentCommitedData.length - 1,
                }"
            >
                <div
                    class="border-none border-bottom-1 border-400 border-dashed flex align-items-center justify-content-between"
                >
                    <span class="font-bold text-lg text-500">{{ line.type }}</span>
                    <Button
                        icon="pi pi-eye"
                        text
                        @click="OpenDetail(line.type_code)"
                    />
                </div>
                <div v-for="(lineSub, ii) in line.data" :key="ii" class="my-2">
                    <div class="flex justify-content-between align-items-center">
                        <div class="font-semibold mb-2">
                            <span>{{ lineSub.label }}:</span>
                            <span class="font-normal ml-3"
                                >{{ formatNumber(lineSub.afterVolumn || 0) }} /
                                {{ lineSub.limit_plan_lbl }} (Lít)</span
                            >
                            <!-- {{ lineSub.current }} -->
                        </div>
                    </div>
                    <PercentBar
                        :arguments="[
                            {
                                label: 'Sản lượng đã tích lũy',
                                value: lineSub.currentVolumn,
                                unit: 'Lít',
                            },
                            {
                                label: 'Sản lượng cộng thêm',
                                value: lineSub.afterVolumn,
                                unit: 'Lít',
                            },
                        ]"
                        :value="lineSub.limit_plan"
                        unit="Lít"
                    ></PercentBar>
                </div>
            </div>
            <!-- {{ props.productsSelected }} -->
            <div class="p-3" v-if="currentCommitedData.length < 1">
                Khách hàng chưa có cam kết
            </div>
        </div>
        <!-- {{ props.commited }} -->
    </div>

    <Dialog
        v-model:visible="visibleConfirm"
        modal
        :header="t('client.commitment_content')"
        maximizable
        class="w-7"
    >
        <div class="flex flex-column gap-4">
            <div
                class="card flex flex-column gap-3"
                poStore.getCommitted
                v-for="(item, index) of poStore.getCommitted?.committedLine.filter(
                    (el) => el.committedType === typeModal
                )"
                :key="index"
            >
                <div class="grid">
                    <div class="col-6 gap-2">
                        <div class="flex gap-2">
                            <label
                                ><strong>
                                    {{t('body.sampleRequest.commitment.application_method_label')}}
                                    <sup class="text-red-500">*</sup></strong
                                ></label
                            >
                            <Tag :value="getFormat(item.committedType)['label']"></Tag>
                        </div>
                    </div>
                    <div class="col-12 p-2">
                        <DataTable
                            showGridlines
                            :value="item.committedLineSub"
                            tableStyle="min-width: 50rem"
                            resizableColumns
                            columnResizeMode="fit"
                            v-model:expandedRows="expandedRows"
                        >
                            <Column expander style="width: 5rem" />
                            <Column
                                v-for="col of DataStruct.structTable.filter(
                                    (el) =>
                                        !el.committedType ||
                                        el.committedType?.includes(item.committedType)
                                )"
                                :key="col.field"
                                :field="col.field"
                                :header="col.header"
                            >
                                <template #body="{ data }">
                                    <div
                                        v-if="col.typeValue == 'Dropdown'"
                                        class="flex gap-2"
                                    >
                                        {{ data[col.field] }}
                                    </div>
                                    <div
                                        v-if="col.typeValue == 'MultiSelect'"
                                        class="flex"
                                    >
                                        <span>
                                            {{ data[col.field]?.[0]?.name }}
                                        </span>
                                        <template v-if="data[col.field]?.length > 1">
                                            <span>, ...</span>
                                            <span class="font-italic">
                                                (+{{ data[col.field]?.length - 1 }})
                                            </span>
                                            <Button
                                                text
                                                :label="t('common.btn_show_more')"
                                                class="p-0 ml-2"
                                                size="small"
                                                @click="
                                                    onClickShowMoreData(
                                                        $event,
                                                        data[col.field]
                                                    )
                                                "
                                            />
                                        </template>
                                    </div>
                                    <span v-if="col.typeValue == 'InputNumber'">{{
                                        Intl.NumberFormat().format(data[col.field])
                                    }}</span>
                                    <div
                                        v-if="col.typeValue == 'Mixed'"
                                        class="flex align-items-center gap-2"
                                    >
                                        <template v-if="data[col.field]">
                                            <span>{{ data[col.field] }}%</span>
                                            <div class="flex gap-2">
                                                <Checkbox
                                                    disabled
                                                    v-model="data[col.sub_field]"
                                                    binary
                                                ></Checkbox>
                                                <span>Quy ra hàng</span>
                                            </div>
                                        </template>
                                    </div>
                                </template>
                            </Column>
                            <template #expansion="sp">
                                <DataTable
                                    v-if="sp.data?.committedLineSubSub?.length"
                                    :value="sp.data?.committedLineSubSub"
                                    tableStyle="max-width: 25rem"
                                    resizableColumns
                                    columnResizeMode="fit"
                                >
                                    <Column
                                        field="code"
                                        header="#"
                                        class="text-right w-1rem"
                                    >
                                        <template #body="{ index }">
                                            <span>{{ index + 1 }}</span>
                                        </template>
                                    </Column>
                                    <Column
                                        field="outPut"
                                        :header="t('client.exceeded_production')"
                                        class="text-right"
                                    >
                                        <template #body="{ data }">
                                            {{ Intl.NumberFormat().format(data.outPut) }}
                                        </template>
                                    </Column>
                                    <Column
                                        field="discount"
                                        :header="t('client.discount')"
                                        class="text-right w-10rem"
                                    >
                                    </Column>
                                </DataTable>
                                <div class="card p-4 font-italic text-500" v-else>
                                    {{t('body.report.NoexcessOutput')}}
                                </div>
                            </template>
                        </DataTable>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    :label="t('common.btn_close')"
                    icon="pi pi-times"
                    severity="secondary"
                    @click="visibleConfirm = false"
                />
            </div>
        </template>
    </Dialog>
    <OverlayPanel ref="opShowMore">
        <Listbox
            :options="showMoreData"
            optionLabel="name"
            class="w-full md:w-20rem"
            listStyle="max-height:250px"
        />
    </OverlayPanel>

    <!-- {{ props.productsSelected }} -->
</template>
<script setup>
import { computed, onMounted, watch, ref, nextTick } from "vue";
import { useI18n } from "vue-i18n";

import API from "../api/api-main";
import { el } from "date-fns/locale";
import { reactive } from "vue";
const { t } = useI18n();
const props = defineProps([
    "customer",
    "payload",
    "productsSelected",
    "commited",
    "tienGiamSanLuong",
]);
import { usePurchaseOrderStore } from "../Pinia/PurchaseOrder";
const poStore = usePurchaseOrderStore();
// -------------------------------------------------
const loading = ref(false);
const opShowMore = ref();
const showMoreData = ref([]);
const onClickShowMoreData = (event, data) => {
    showMoreData.value = data;
    opShowMore.value?.toggle(event);
};

const getCurrentQuarter = () => {
    const month = new Date().getMonth(); // Tháng từ 0-11
    return Math.floor(month / 3) + 1; // Chia cho 3 và làm tròn xuống để lấy quý (1-4)
};

const lineType = {
    Q: t('client.quarter'),
    Y: t('client.month'),
    // 'P' (package) key not present in en.json — keep literal or add key if needed
    P: "Gói",
};

const volumnSelected = (line, type) => {
    // logic filter selected product on customer's committed
    // return (
    //     props.productsSelected
    //         ?.filter((item) => item.industry?.name === line)
    //         .reduce((acc, curr) => acc + curr.packing?.volumn * curr.quantity, 0) || 0
    // );

    return 0;
};
const getQorM = (type_code) => {
    if (type_code == "Q") {
        return getCurrentQuarter();
    } else if (type_code == "Y") {
        return new Date().getMonth() + 1;
    }
    return 0;
};
const getLimitPlan = (type_code) => {
    if (type_code == "Q") {
        return `quarter${getQorM(type_code)}`;
    } else if (type_code == "Y") {
        return `month${getQorM(type_code)}`;
    } else {
        return "package";
    }
};
const currentCommitedData = computed(() => {
    // const dataLines = committedModel.value?.committedLine;
    const dataLines = poStore.getCommitted?.committedLine;
    if (!dataLines) return [];
    const result = [];
    for (const line of dataLines) {
        const lineSubs = [];
        for (const lineSub of line.committedLineSub) {
            const limit_plan = lineSub[getLimitPlan(line.committedType)];
            const _lineSub = {
                label: lineSub.industryName || "line",
                total: lineSub.total,
                limit_plan: limit_plan,
                limit_plan_lbl: formatNumber(limit_plan),
                current: lineSub.currentVolumn
                    ? parseFloat(lineSub.currentVolumn.toFixed(2))
                    : 0,
                currentVolumn: lineSub.currentVolumn,
                afterVolumn: lineSub.afterVolumn,
                lbl_current: formatNumber(lineSub.currentVolumn),
                industryId: lineSub.industry.id,
                brandIds: lineSub.brand?.map((row) => row.id) || [],
                itemTypeIds: lineSub.itemTypes?.map((row) => row.id) || [],
            };
            lineSubs.push(_lineSub);
        }
        const lineMapped = {
            type_code: line.committedType,
            type:
                lineType[line.committedType] +
                " " +
                (getQorM(line.committedType) > 0 ? getQorM(line.committedType) : ""),
            data: lineSubs,
        };
        result.push(lineMapped);
    }
    return result;
});

// -------------------------------------------------
let type = {
    Q: { label: t('client.quarter') },
    Y: { label: "Năm" }, // add t('client.year') if you add that key to en.json
    P: { label: "Gói" },
};
const getFormat = (stt) => {
    return type[stt];
};
const typeModal = ref("");
const expandedRows = ref([]);
const DataStruct = ref({
    structTable: [
        {
            field: "industryName",
            header: t('client.industry'),
            require: true,
            typeValue: "Dropdown",
            placeholder: t('client.input_industry'),
        },
        {
            field: "brand",
            header: t('client.brand'),
            require: true,
            typeValue: "MultiSelect",
            placeholder: t('client.input_brand'),
        },
        {
            field: "itemTypes",
            header: t('client.product_type'),
            require: true,
            typeValue: "MultiSelect",
            placeholder: t('client.input_product'),
        },
        {
            field: "quarter1",
            header: t('body.sampleRequest.commitment.table_header_q1_volume'),
            typeValue: "InputNumber",
            placeholder: t('client.input_quarter_1'),
            committedType: ["Q"],
        },
        {
            field: "quarter2",
            header: t('body.sampleRequest.commitment.table_header_q2_volume'),
            typeValue: "InputNumber",
            placeholder: t('client.input_quarter_2'),
            committedType: ["Q"],
        },
        {
            field: "quarter3",
            header: t('body.sampleRequest.commitment.table_header_q3_volume'),
            typeValue: "InputNumber",
            placeholder: t('client.input_quarter_3'),
            committedType: ["Q"],
        },
        {
            field: "quarter4",
            header: t('body.sampleRequest.commitment.table_header_q4_volume'),
            typeValue: "InputNumber",
            placeholder: t('client.input_quarter_4'),
            committedType: ["Q"],
        },
        {
            field: "package",
            header: t('client.total_output'),
            typeValue: "InputNumber",
            placeholder: t('client.input_total'),
            committedType: ["P"],
        },
        {
            field: "month1",
            header: t('body.report.table_header_month_1'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth1'),
            committedType: ["Y"],
        },
        {
            field: "month2",
            header: t('body.report.table_header_month_2'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth2'),
            committedType: ["Y"],
        },
        {
            field: "month3",
            header: t('body.report.table_header_month_3'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth3'),
            committedType: ["Y"],
        },
        {
            field: "month4",
            header: t('body.report.table_header_month_4'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth4'),
            committedType: ["Y"],
        },
        {
            field: "month5",
            header: t('body.report.table_header_month_5'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth5'),
            committedType: ["Y"],
        },
        {
            field: "month6",
            header: t('body.report.table_header_month_6'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth6'),
            committedType: ["Y"],
        },
        {
            field: "month7",
            header: t('body.report.table_header_month_7'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth7'),
            committedType: ["Y"],
        },
        {
            field: "month8",
            header: t('body.report.table_header_month_8'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth8'),
            committedType: ["Y"],
        },
        {
            field: "month9",
            header: t('body.report.table_header_month_9'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth9'),
            committedType: ["Y"],
        },
        {
            field: "month10",
            header: t('body.report.table_header_month_10'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth10'),
            committedType: ["Y"],
        },
        {
            field: "month11",
            header: t('body.report.table_header_month_11'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth11'),
            committedType: ["Y"],
        },
        {
            field: "month12",
            header: t('body.report.table_header_month_12'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterMonth12'),
            committedType: ["Y"],
        },
        {
            field: "total",
            header: t('body.sampleRequest.commitment.table_header_total_volume'),
            typeValue: "InputNumber",
            disabled: true,
            placeholder: t('client.input_total'),
            committedType: ["Y", "Q"],
        },
        {
            field: "discountMonth",
            sub_field: "isCvMonth",
            header: t('body.report.monthlyDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterMonthlyDiscount'),
            committedType: ["Y"],
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: t('body.report.quarterlyDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterQuarterlyDiscount'),
            committedType: ["Q"],
        },
        {
            field: "threeMonthDiscount",
            sub_field: "isCvThreeMonth",
            header: t('body.report.threeMonthDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterThreeMonthDiscount'),
            committedType: ["Y"],
        },
        {
            field: "sixMonthDiscount",
            sub_field: "isCvSixMonth",
            header: t('body.report.sixMonthDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterSixMonthDiscount'),
            committedType: ["Q", "Y"],
        },
        {
            field: "nineMonthDiscount",
            sub_field: "isCvNineMonth",
            header: t('body.report.enterNineMonthDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterNineMonthDiscount'),
            committedType: ["Q", "Y"],
        },
        {
            field: "discountYear",
            sub_field: "isCvYear",
            header: t('body.report.yearlyDiscount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterDiscount'),
            committedType: ["Q", "Y"],
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: t('body.report.discount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterDiscountGeneral'),
            committedType: ["P"],
        },
    ],
    structTableChill: [
        {
            field: "outPut",
            header: t('body.report.table_header_exceeding_volume_bonus'),
            typeValue: "InputNumber",
            placeholder: t('body.report.enterExcessOutput'),
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: t('body.report.discount'),
            typeValue: "Mixed",
            placeholder: t('body.report.enterDiscount'),
        },
    ],
});
const visibleConfirm = ref(false);
const committedModel = ref();
const formatNumber = (num) => {
    if (!num) return 0;
    return Intl.NumberFormat().format(num);
};
const OpenDetail = (type) => {
    typeModal.value = type;
    visibleConfirm.value = true;
};

const tiemGiam = ref(null);
var flag = true;
const cmtType = {
    Q: "Quý",
    Y: "Năm",
};
const currentQYNum = {
    Q: Math.floor(new Date().getMonth() / 3) + 1,
    Y: new Date().getMonth() + 1,
};
const getProVal = (type) => {
    if (type == "Q") return `bonusQuarter${currentQYNum[type]}`;
    else if (type == "Y") return `bonusMonth${currentQYNum[type]}`;
    else return "";
};

const getByIdCommeted = (id, data = []) => {
    return;
    if (!flag) return;
    if (id) {
        committedModel.value = {};
        loading.value = true;
        API.add(`Commited/checking?cardId=${id}`, data)
            .then((res) => {
                committedModel.value = res.data;
                // old

                // new
                const info = {
                    name: "",
                    label: "Thưởng sản lượng",
                    total: 0,
                    yearTotal: 0,
                    quarterTotal: 0,
                    visible: false,
                    lines: [],
                };
                if (res.data?.committedLine.length) {
                    // debugger
                    for (let line of res.data?.committedLine) {
                        const _line = {
                            label: cmtType[line.committedType]
                                ? `Thưởng sản lượng ${cmtType[line.committedType]} ${
                                      currentQYNum[line.committedType]
                                  }`
                                : "Unknown",
                            time_num: currentQYNum[line.committedType],
                            value: 0,
                        };

                        const line3M = {
                            label: "Thưởng 3 tháng",
                            value: 0,
                        };
                        const line6M = {
                            label: "Thưởng 6 tháng",
                            value: 0,
                        };
                        const line9M = {
                            label: "Thưởng 9 tháng",
                            value: 0,
                        };
                        const line1Y = {
                            label: "Thưởng năm",
                            value: 0,
                        };
                        const lineOver = {
                            label: "Thưởng sản lượng vượt",
                            value: 0,
                        };

                        for (let subLine of line.committedLineSub) {
                            if (subLine[getProVal(line.committedType)]) {
                                _line.value += subLine[getProVal(line.committedType)];
                                info.total += subLine[getProVal(line.committedType)];
                                if (line.committedType == "Y") {
                                    info.yearTotal +=
                                        subLine[getProVal(line.committedType)];
                                }
                                if (line.committedType == "Q") {
                                    info.quarterTotal +=
                                        subLine[getProVal(line.committedType)];
                                }
                            }
                            if (subLine.threeMonthBonus) {
                                line3M.value += subLine.threeMonthBonus;
                                info.total += subLine.threeMonthBonus;
                                if (line.committedType == "Y") {
                                    info.yearTotal += subLine.nineMonthBonus;
                                }
                                if (line.committedType == "Q") {
                                    info.quarterTotal += subLine.nineMonthBonus;
                                }
                            }
                            if (subLine.sixMonthBonus) {
                                line6M.value += subLine.sixMonthBonus;
                                info.total += subLine.sixMonthBonus;
                                if (line.committedType == "Y") {
                                    info.yearTotal += subLine.sixMonthBonus;
                                }
                                if (line.committedType == "Q") {
                                    info.quarterTotal += subLine.sixMonthBonus;
                                }
                            }
                            if (subLine.nineMonthBonus) {
                                line9M.value += subLine.nineMonthBonus;
                                info.total += subLine.nineMonthBonus;
                                if (line.committedType == "Y") {
                                    info.yearTotal += subLine.nineMonthBonus;
                                }
                                if (line.committedType == "Q") {
                                    info.quarterTotal += subLine.nineMonthBonus;
                                }
                            }
                            if (
                                subLine.afterVolumn >= subLine.total &&
                                subLine.yearBonus
                            ) {
                                line1Y.value += subLine.yearBonus;
                                info.total += subLine.yearBonus;
                                if (line.committedType == "Y") {
                                    info.yearTotal += subLine.nineMonthBonus;
                                }
                                if (line.committedType == "Q") {
                                    info.quarterTotal += subLine.nineMonthBonus;
                                }
                            }
                            // Thưởng sản lượng vượt
                            if (subLine.committedLineSubSub.length) {
                                for (let subSubLine of subLine.committedLineSubSub) {
                                    if (subLine.afterVolumn >= subSubLine.outPut) {
                                        lineOver.value += subSubLine.bonusTotal;
                                        info.total += subSubLine.bonusTotal;
                                        if (line.committedType == "Y") {
                                            info.yearTotal += subSubLine.bonusTotal;
                                        }
                                        if (line.committedType == "Q") {
                                            info.quarterTotal += subSubLine.bonusTotal;
                                        }
                                    }
                                }
                            }
                        }

                        info.lines.push(
                            ...[_line, line3M, line6M, line9M, line1Y, lineOver]
                        );
                    }

                    tiemGiam.value = info;
                    // set to purchase order store
                    poStore.setData(
                        {
                            quy: info.quarterTotal,
                            nam: info.yearTotal,
                        },
                        "CK"
                    );
                }
            })
            .catch((error) => {
                const info = {
                    name: "",
                    label: "Thưởng sản lượng",
                    total: 0,
                    yearTotal: 0,
                    quarterTotal: 0,
                    visible: false,
                    lines: [],
                };
                tiemGiam.value = info;
                // set to purchase order store
                poStore.setData(
                    {
                        quy: info.quarterTotal,
                        nam: info.yearTotal,
                    },
                    "CK"
                );
            })
            .finally(() => {
                flag = true;
                loading.value = false;
            });
    }
};

const payloadItems = ref([]);

const onChoseProduct = (items) => { 
    payloadItems.value = items?.map((row) => {
        return {
            itemId: row.id,
            quantity: row.quantity,
            price: row.price,
        };
    });
    tiemGiam.value = null;
    if (payloadItems.value.length && props.customer?.id) {
        getByIdCommeted(props.customer?.id, payloadItems.value);
    }
};

defineExpose({
    onChoseProduct,
    tiemGiam,
});

watch(
    () => props.customer?.id,
    (id) => {
        if (id) { 
            // onChoseProduct();
            getByIdCommeted(id, payloadItems.value);
        } else {
            tiemGiam.value = null;
        }
    }
);
watch(
    () => props.productsSelected?.length,
    (value) => {
        if (value) onChoseProduct(props.productsSelected);
    }
);

onMounted(() => {
    nextTick(() => {
        getByIdCommeted(props.customer?.id, payloadItems.value);
    });
    // setTimeout(() => {
    // }, 1000);
});
</script>
<style scoped></style>
