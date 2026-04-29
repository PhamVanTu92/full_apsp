<template>
    <InputGroup>
        <AutoComplete v-model="data" multiple :suggestions="DataItem" optionLabel="name" @complete="SearchItemData"
            @change="FunChangeItem" class="w-full" :placeholder="t('common.placeholder_select_goods')" />
        <InputGroupAddon @click="ChooseGroupItem">
            <i class="pi pi-list"></i>
        </InputGroupAddon>
    </InputGroup>
    <Dialog v-model:visible="dialogGroupItem" modal :header="t('common.dialog_select_group')" :style="{ width: '500px' }">
        <div class="p-2">
            <div class="mb-3">
                <InputGroup>
                    <InputGroupAddon>
                        <i class="pi pi-search"></i>
                    </InputGroupAddon>
                    <InputText :placeholder="t('common.placeholder_search_product_group')" @keyup="keyUpSreachGItem()" />
                </InputGroup>
            </div>
            <Tree v-model:selectionKeys="dataNode" :value="dataGroupItem" selectionMode="checkbox"
                class="w-full p-0 m-0">
            </Tree>
        </div>
        <template #footer>
            <div class="flex justify-content-between w-full">
                <Button :label="t('common.btn_deselect_all')" text severity="info" />
                <div class="flex gap-3">
                    <Button :label="t('common.btn_skip')" icon="pi pi-ban" severity="secondary" @click="dialogGroupItem = false" />
                    <Button :label="t('common.btn_confirm')" icon="pi pi-check-circle" @click="saveData()" />
                </div>
            </div>
        </template>
    </Dialog>
</template>
<script setup>
import { ref, watchEffect } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
import { getCurrentInstance } from "vue";
import API from "../api/api-main-1";
import { useToast } from "primevue/usetoast";
import { debounce } from "lodash";

const props = defineProps({
    data_req: Object,
});
const emit = defineEmits(["change"]);
const { proxy } = getCurrentInstance();
const toast = useToast();
const dialogGroupItem = ref(false);
const dataGroupItem = ref([]);
const DataItem = ref([]);
const data = ref([]);
const dataNode = ref([]);
const keySreachGItem = ref("");
const ChooseGroupItem = async () => {
    try {
        const res = await API.get("ItemGroup");
        if (res.data.itemGroup) dataGroupItem.value = res.data.itemGroup;
        ConvertData(dataGroupItem.value);
    } catch (error) {
        proxy.$notify("E", error, toast);
    } finally {
        dialogGroupItem.value = true;
    }
};

const ConvertData = (data, parentKey = "") => {
    for (let index = 0; index < data.length; index++) {
        const item = data[index];
        item.key = item.parentId != null ? item.parentId + "-" + item.id : item.id;
        item.key = parentKey != "" ? parentKey + "-" + item.id : item.key;
        item.label = item.itmsGrpName;
        if (item.child.length) {
            item.children = item.child;
            ConvertData(item.child, item.key);
        }
    }
};

const getItemData = async (key) => {
    try {
        const res = await API.get(`Item/search/${key}`);
        for (let index = 0; index < res.data.length; index++) {
            const element = res.data[index];
            element.name = element.itemName;
        }
        DataItem.value = res.data;
    } catch (error) { }
};
const debouncedFetchProduct = debounce(getItemData, 300);

const SearchItemData = (event) => {
    debouncedFetchProduct(event.query);
};
const saveData = () => {
    data.value = [];
    const dataArr = SetArrNode(dataNode.value);
    for (let index = 0; index < dataArr.length; index++) {
        const element = dataArr[index];
        getDataGroupItem(dataGroupItem.value, element);
    }
    dialogGroupItem.value = false;
};

const getDataGroupItem = (d, key) => {
    d.forEach((item) => {
        if (key.toString() == item.key.toString()) {
            item.name = "Nhóm: " + item.itmsGrpName;
            item.type = "G";
            item.itemGroupId = item.id;
            data.value.push(item);
        } else {
            if (item.children) {
                getDataGroupItem(item.children, key);
            }
        }
    });
};

const SetArrNode = (data) => {
    const arr = [];
    const shouldAddToArr = (key, item, length) => {
        return (
            key.split("-").length === length &&
            item.checked &&
            !item.partialChecked &&
            !arr.some(
                (val) =>
                    val.split("-")[0] === key.split("-")[0] && val.split("-").length !== length
            )
        );
    };
    for (const key in data) {
        if (data.hasOwnProperty(key)) {
            const item = data[key];
            if (
                shouldAddToArr(key, item, 1) ||
                shouldAddToArr(key, item, 2) ||
                shouldAddToArr(key, item, 3)
            ) {
                arr.push(key);
            }
        }
    }
    return arr;
};

const FunChangeItem = (e) => {
    try {
        if (data.value[data.value.length - 1].itemCode) {
            const count_item = data.value.filter((val) => {
                return val.itemCode == data.value[data.value.length - 1].itemCode;
            });
            if (count_item.length > 1) {
                data.value.splice(data.value.length - 1, 1);
            }
            const count_Group = data.value.filter((val) => {
                return val.type == "G" || val.type == "IG";
            });
            if (count_Group.length > 0) {
                data.value.splice(0, data.value.length - 1);
            }
        }
    } catch {

    }
    const id = getItemRemove(SetArrNode(dataNode.value), data.value);
    if (data.value.length != 0) {
        if (data.value[0].type == "G" && data.value[data.value.length - 1].itemCode) {
            data.value = [data.value[data.value.length - 1]];
            dataNode.value = {};
            dataGroupItem.value = [];
        }
        if (data.value[0].itemCode) return;
    }
    if (id != "") {
        if (id.split("-").length == 1) {
            dataNode.value[id].checked = false;
            setFalse(id, dataNode.value);
        } else {
            dataNode.value[id].checked = false;
            if (!checkNode(id.split("-")[0], dataNode.value)) {
                setFalse(id.split("-")[0], dataNode.value);
            }
        }
    }
};

const getItemRemove = (dataOrigin, dataNew) => {
    let ArrRemove = [];
    let data = "";
    for (let index = 0; index < dataOrigin.length; index++) {
        const ITEM = dataOrigin[index];
        ArrRemove = dataNew.filter((val) => {
            return val.key == ITEM;
        });
        if (ArrRemove.length == 0) {
            data = ITEM;
            dataOrigin.splice(index, 1);
        }
    }
    return data;
};

const setFalse = (id, data) => {
    for (const key in data) {
        if (data.hasOwnProperty(key)) {
            let key_split = key.split("-");
            if (parseInt(id) == parseInt(key_split[0])) {
                data[key].partialChecked = false;
                data[key].checked = false;
            }
        }
    }
};

const checkNode = (id, data) => {
    for (const key in data) {
        if (data.hasOwnProperty(key)) {
            let key_split = key.split("-");
            if (parseInt(id) == parseInt(key_split[0]) && data[key].checked == true) {
                return true;
            }
        }
    }
    return false;
};

const updateData = async (data) => {
    try {
        const res = await API.get("ItemGroup");
        if (res.data.itemGroup) dataGroupItem.value = res.data.itemGroup;
        ConvertData(dataGroupItem.value);
    } catch (error) {
    } finally {
        dataNode.value = {};
        data.forEach((item) => {
            if (item.itemGroupId) {
                let key = FindByID(item.itemGroupId, dataGroupItem.value).key;
                key = key.toString().split("-");
                if (key.length == 3) {
                    const dataNew = {};
                    dataNew[key[0]] = { checked: false, partialChecked: true };
                    dataNew[key[0] + "-" + key[1]] = {
                        checked: false,
                        partialChecked: true,
                    };
                    dataNew[key[0] + "-" + key[1] + "-" + key[2]] = {
                        checked: true,
                        partialChecked: false,
                    };
                    Object.assign(dataNode.value, dataNew);
                }
                if (key.length == 2) {
                    const dataNew = {};
                    dataNew[key[0]] = { checked: false, partialChecked: true };
                    dataNew[key[0] + "-" + key[1]] = {
                        checked: true,
                        partialChecked: false,
                    };
                    SetValueTrueV2(key[1], dataGroupItem.value);
                    Object.assign(dataNode.value, dataNew);
                }

                if (key.length == 1) {
                    const dataNew = {};

                    dataNew[key[0]] = { checked: true, partialChecked: false };
                    Object.assign(dataNode.value, dataNew);
                    SetValueTrue(key[0], dataGroupItem.value);
                }
            }
        });
    }
};

const FindByID = (id, data) => {
    for (let index = 0; index < data.length; index++) {
        const item = data[index];
        if (item.id == id) {
            return item;
        }
        if (item.children) {
            const found = FindByID(id, item.children);
            if (found) return found;
        }
    }
    return null;
};

const SetValueTrueV2 = (id, data) => {
    const Arr = FindChild(id, data);
    Arr.forEach((item) => {
        let key = FindByID(item.id, data).key;
        key = key.toString().split("-");
        if (key.length == 3) {
            const dataNew = {};
            dataNew[key[0] + "-" + key[1] + "-" + key[2]] = {
                checked: true,
                partialChecked: false,
            };
            Object.assign(dataNode.value, dataNew);
        }
    });
};

const SetValueTrue = (id, data) => {
    const Arr = FindChild(id, data);
    Arr.forEach((item) => {
        let key = FindByID(item.id, data).key;
        key = key.toString().split("-");
        if (key.length == 2) {
            const dataNew = {};
            dataNew[key[0] + "-" + key[1]] = {
                checked: true,
                partialChecked: false,
            };
            Object.assign(dataNode.value, dataNew);
            SetValueTrueV2(key[1], data);
        }
    });
};

const FindChild = (id, data) => {
    const tem = [];
    for (let index = 0; index < data.length; index++) {
        const item = data[index];
        if (parseInt(item.parentId) === parseInt(id)) {
            tem.push(item);
        }
        if (item.children && item.children.length > 0) {
            tem.push(...FindChild(id, item.children));
        }
    }
    return tem;
};

watchEffect(() => {
    emit("change", data.value);
});

if (props.data_req.length > 0) {
    data.value = props.data_req;
    data.value.forEach((item) => {
        item.name = item.itmsGrpName ? item.itmsGrpName : item.itemName;
    });
    updateData(data.value);
}

const keyUpSreachGItem = () => { };
</script>
