<template>
    <div class="card">
        <div class="flex justify-content-between mb-3">
            <div class="py-2 text-green-700 text-xl font-semibold">
                Danh mục sản phẩm đặc thù
            </div>
            <div class="flex gap-3">
                <Button
                    v-if="!editMode"
                    :disabled="editMode"
                    @click="editMode = true"
                    icon="pi pi-pencil"
                    label="Chỉnh sửa"
                    text
                />
                <template v-else>
                    <Button
                        @click="onClickCancel"
                        icon="pi pi-times"
                        label="Huỷ"
                        severity="secondary"
                        text
                    />
                    <Button
                        :loading="loading"
                        @click="onClickSave"
                        icon="pi pi-save"
                        label="Lưu"
                    />
                </template>
            </div>
        </div>
        <hr class="m-0" />
        <DataTable
            :value="
                products_groups.filter((el) => el.type === tableType && el.status != 'D')
            "
        >
            <Column field="" header="#" style="width: 5rem">
                <template #body="{ index }">
                    {{ ++index }}
                </template>
            </Column>
            <Column :header="tableType == 'I' ? 'Tên hàng hóa' : 'Nhóm sản phẩm'">
                <template #body="sp">
                    <span>{{ sp.data.typeName }}</span>
                </template>
            </Column>
            <Column style="width: 3rem" v-if="editMode">
                <template #body="sp">
                    <Button
                        @click="onClickRemoveRow(sp)"
                        icon="pi pi-trash"
                        severity="danger"
                        text
                    />
                </template>
            </Column>
            <template #header v-if="editMode">
                <input-group class="w-30rem">
                    <AutoComplete
                        @item-select="onSelectProduct"
                        v-model="itemSelected"
                        optionLabel="itemName"
                        :suggestions="suggestions"
                        @complete="onSearch"
                        placeholder="Chọn sản phẩm"
                    >
                        <template #option="slotProps">
                            <div class="flex align-items-center gap-3 w-30rem">
                                <img
                                    :src="
                                        slotProps.option.itM1[0]?.filePath ||
                                        'https://placehold.co/50'
                                    "
                                    style="width: 50px"
                                />
                                <div>{{ slotProps.option.itemName }}</div>
                            </div>
                        </template>
                    </AutoComplete>
                    <Button
                        @click="visible.prdctGrp = true"
                        icon="pi pi-list"
                        severity="secondary"
                    />
                </input-group>
            </template>
            <template #empty>
                <div class="p-5 text-center">Chưa có dữ liệu để hiển thị</div>
            </template>
        </DataTable>

        <!-- {{ DATA.brands }} -->
    </div>

    <Dialog
        @hide="onHideDialog"
        v-model:visible="visible.prdctGrp"
        modal
        header="Chọn nhóm sản phẩm"
        style="width: 30rem"
    >
        <Tree
            v-model:selectionKeys="selectedPrdctGrp"
            scrollHeight="20rem"
            :value="DATA.itemGroups"
            :filter="true"
            filterMode="lenient"
            :metaKeySelection="true"
            selectionMode="checkbox"
            class="w-full"
        >
        </Tree>
        <!-- {{ selectedPrdctGrp }} -->
        <div class="flex justify-content-between py-3">
            <span class="font-italic"
                >Đã chọn
                {{
                    Object.keys(selectedPrdctGrp)?.filter(
                        (el) => selectedPrdctGrp[el].checked
                    ).length
                }}
                nhóm
            </span>
            <Button
                @click="
                    () => {
                        selectedPrdctGrp = {};
                    }
                "
                class="p-0"
                label="Bỏ chọn"
                severity="danger"
                text
            />
        </div>
        <div class="flex justify-content-end gap-3">
            <Button
                @click="() => (visible.prdctGrp = false)"
                label="Đóng"
                icon="pi pi-times"
                severity="secondary"
            />
            <Button @click="onSelectGroup" label="Chọn" icon="pi pi-check"/>
        </div>
    </Dialog>
</template>

<script setup>
import { onMounted, ref, reactive } from "vue";
const emits = defineEmits(["on-save"]);

const editMode = ref(false);
const props = defineProps({
    setup: {
        API: {
            type: Object,
            required: true,
        },
        states: {
            type: Object,
            required: true,
        },
        toast: {
            type: Object,
            required: true,
        },
    },
});

const modelStates = reactive({
    brand: props.setup.modelStates.brand?.split(",").map((el) => parseInt(el)) || [],
    industry:
        props.setup.modelStates.industry?.split(",").map((el) => parseInt(el)) || [],
    isAllBrand: props.setup.modelStates.isAllBrand,
    isAllIndustry: props.setup.modelStates.isAllIndustry,
});

const visible = reactive({
    prdctGrp: false,
});

const DATA = reactive({
    brands: [],
    industries: [],
    itemGroups: [],
});

const tableType = ref("I");
const defaultTableType = ref();

const itemSelected = ref();
const suggestions = ref([]);
const products_groups = ref([]);
const onSearch = (event) => {
    props.setup.API.get(`item?search=${event.query}`)
        .then((res) => {
            suggestions.value = res.data.items;
        })
        .catch((error) => {
            console.error(error);
        });
};
const onSelectProduct = (event) => {
    tableType.value = "I";
    let idx = products_groups.value.find(
        (el) => el.typeId == event.value.id && el.type == "I"
    );
    if (!idx) {
        products_groups.value.push({
            typeId: event.value.id,
            type: "I",
            status: "A",
            typeName: event.value.itemName,
            typeCode: event.value.itemCode,
        });
    }
    itemSelected.value = null;
};

const selectedPrdctGrp = ref({});
const onSelectGroup = () => {
    tableType.value = "T";
    let _keys = [];
    Object.keys(selectedPrdctGrp.value).forEach((key) => {
        if (selectedPrdctGrp.value[key].checked) {
            _keys.push(key);
        }
    });
    const groups = ConvertToArray(DATA.itemGroups, _keys).map((el) => ({
        typeId: el.id,
        type: "T",
        status: "A",
        typeName: el.itmsGrpName,
        typeCode: el.itmsGrpName,
    }));

    let groupsFiltered = groups.filter(
        (el) => !products_groups.value.map((x) => x.typeId).includes(el.typeId)
    );
    products_groups.value.push(...groupsFiltered);
    visible.prdctGrp = false;
};

const onClickRemoveRow = (sp) => {
    if (sp.data.status == "A") {
        products_groups.value.splice(sp.index, 1);
    } else {
        sp.data.status = "D";
    }
};

const loading = ref(false);
const onClickSave = async () => {
    try {
        loading.value = true;
        let payload = { ...modelStates };
        payload.id = props.setup.modelStates.id;

        payload.brand = payload.isAllBrand ? null : payload.brand.toString();
        payload.industry = payload.isAllIndustry
            ? null
            : (payload.industry = payload.industry.toString());

        products_groups.value.forEach((el, i) => {
            if (
                defaultTableType.value != tableType.value &&
                el.type == defaultTableType.value
            ) {
                if (el.status == "A") {
                    products_groups.value.splice(i, 1);
                } else {
                    el.status = "D";
                }
            }
        });

        payload.crD2 = products_groups.value;

        const formData = new FormData();
        formData.append("item", JSON.stringify(payload));

        const res = await props.setup.API.update(
            "customer/" + props.setup.modelStates.id,
            formData
        );
        editMode.value = false;
        res.data.crD2 = res.data.crD2.map((el) => ({ ...el, status: null }));
        props.setup.toast.add({
            severity: "success",
            summary: "Thông báo",
            detail: "Cập nhật dữ liệu thành công",
            life: 3000,
        });
        emits("on-save", res.data);
        bindingDataAfterSave(res.data);
    } catch (error) {
        console.error(error);
        onClickCancel();
        props.setup.toast.add({
            severity: "error",
            summary: "Thông báo",
            detail: error.message,
            life: 3000,
        });
    } finally {
        loading.value = false;
    }
};

const bindingDataAfterSave = (response) => {
    modelStates.brand = response.brand
        ?.split(",")
        .map((el) => parseInt(el) || null)
        .filter((el) => el);
    modelStates.industry = response.industry
        ?.split(",")
        .map((el) => parseInt(el) || null)
        .filter((el) => el);
    modelStates.isAllBrand = response.isAllBrand;
    modelStates.isAllIndustry = response.isAllIndustry;
    products_groups.value = response.crD2;
};

const BindingDataProps = () => {
    modelStates.brand = props.setup.modelStates.brand
        ?.split(",")
        .map((el) => parseInt(el) || null)
        .filter((el) => el);
    modelStates.industry = props.setup.modelStates.industry
        ?.split(",")
        .map((el) => parseInt(el) || null)
        .filter((el) => el);
    modelStates.isAllBrand = props.setup.modelStates.isAllBrand;
    modelStates.isAllIndustry = props.setup.modelStates.isAllIndustry;
};

const onClickCancel = () => {
    editMode.value = false;
    BindingDataProps();
    products_groups.value = products_groups.value.filter((el) => el.status != "A");
    if (tableType.value == "I") {
        products_groups.value = props.setup.modelStates.crD2?.map((el) => ({
            ...el,
            itemName: el.typeName,
        }));
    }
    if (products_groups.value.length < 1) {
        tableType.value = "I";
    } else {
        tableType.value = products_groups.value[0]?.type || "I";
    }
    selectedPrdctGrp.value = { ...defaultSelectGrp.value };
};

const defaultSelectGrp = ref({});
onMounted(() => {
    props.setup.API.get("brand/getall")
        .then((res) => {
            DATA.brands = res.data;
        })
        .catch((error) => {
            console.error(error);
        });

    props.setup.API.get("industry/getall")
        .then((res) => {
            DATA.industries = res.data;
        })
        .catch((error) => {
            console.error(error);
        });

    BindingDataProps();
    props.setup.API.get("ItemGroup")
        .then((res) => {
            DATA.itemGroups = ConvertToTree(res.data.itemGroup, null);
            products_groups.value = props.setup.modelStates.crD2;

            const ids = products_groups.value.map((el) => el.typeId);
            tableType.value = products_groups.value[0]?.type;
            defaultTableType.value = products_groups.value[0]?.type;
            if (tableType.value == "T") {
                defaultSelectGrp.value = GenerateSelected(DATA.itemGroups, ids);
                selectedPrdctGrp.value = { ...defaultSelectGrp.value };
            }
        })
        .catch((error) => {
            console.error(error);
        });
});

const onHideDialog = () => {
    // selectedPrdctGrp.value = {}
    selectedPrdctGrp.value = { ...defaultSelectGrp.value };
};

const ConvertToTree = (array, fatherKey) => {
    let tree = [];
    if (array) {
        tree = array.map((el) => {
            let key = fatherKey ? `${fatherKey}-${el.id}` : `${el.id}`;
            let item = {
                key: key,
                label: el.itmsGrpName,
                ...el,
            };
            item.children = ConvertToTree(el.child, key);
            return item;
        });
    }
    return tree;
};

const ConvertToArray = (tree, keys) => {
    let arr = [];
    if (tree) {
        tree.forEach((el) => {
            if (keys.includes(el.key)) {
                arr.push(el);
            }
            if (el.children) {
                arr = [...arr, ...ConvertToArray(el.children, keys)];
            }
        });
    }
    return arr;
};

const GenerateSelected = (trees, ids) => {
    let result = {};
    for (let tree of trees) {
        if (ids.includes(tree.id)) {
            result[tree.key] = {
                checked: true,
                partialChecked: false,
            };
        }
        if (tree.children) {
            Object.assign(result, GenerateSelected(tree.children, ids));
        }
    }
    return result;
};
</script>

<style scoped></style>
