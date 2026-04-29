import { ref, onBeforeMount } from 'vue';
import API from '@/api/api-main';
import { uniqBy, cloneDeep, groupBy } from 'lodash';
import tanDev from 'lodash';
import format from '@/helpers/format.helper';
import { FilterStore } from '@/Pinia/FilterPromotion';
import { useGlobal } from '@/services/useGlobal';
import { useRouter, useRoute } from 'vue-router';
import {
    getPromotionType,
    getSubType,
    getFollowBy,
    getFollowBy2,
    getAddType,
    getComparisonOptions,
    defaultPayload,
    defaultPromotionLine,
    defaultPromotionLineSub,
    defaultPromotionLineSubSub,
    getOptionAddValueType
} from '../options';
import { useI18n } from 'vue-i18n';

export function usePromotionEdit() {
    const { t } = useI18n();
    const { toast, FunctionGlobal } = useGlobal();
    const router = useRouter();
    const route = useRoute();

    // ==================== STATE ====================
    const filterStore = FilterStore();
    const visible = ref(false);
    const isFilterPro = ref(false);
    const submited = ref(false);
    const payload = ref();
    const response = ref();
    const loading = ref(false);
    const ItemData = ref([]);
    const ItemType = ref([]);
    const selectedItemType = ref([]);
    const Hierarchy = ref();

    // ==================== OPTIONS ====================
    const optionAddValueType = ref(getOptionAddValueType(t));
    const optionAddType = ref(getAddType(t));
    const optionFollowBy = ref(getFollowBy(t));
    const optionFollowBy2 = ref(getFollowBy2(t));
    const optionSubType = ref(getSubType(t));
    const optionPromotionType = ref(getPromotionType(t));
    const comparisonOptions = ref(getComparisonOptions(t));

    // ==================== INIT ====================
    const initData = () => {
        payload.value = cloneDeep(defaultPayload);
        const promotionLine = cloneDeep(defaultPromotionLine);
        const promotionLineSub = cloneDeep(defaultPromotionLineSub);
        promotionLine.promotionLineSub.push(promotionLineSub);
        payload.value.promotionLine.push(promotionLine);
    };
    initData();

    onBeforeMount(async () => {
        try {
            loading.value = true;
            await getHierarchy();
            if (route?.params?.id) {
                await getById(route?.params?.id);
            }
        } catch (error) {
            console.error(error);
        } finally {
            loading.value = false;
        }
    });

    // ==================== API ====================
    const getById = async (id) => {
        try {
            const res = await API.get(`Promotion/${id}`);
            if (res.data) {
                response.value = res.data;
                const data = res.data;
                data.promotionBrandClone = cloneDeep(data.promotionBrand);
                data.promotionBrand = data.promotionBrand.map((e) => e.brandId);

                data.promotionIndustryClone = cloneDeep(data.promotionIndustry);
                data.promotionIndustry = data.promotionIndustry.map((e) => e.industryId);

                data.promotionCustomerClone = cloneDeep(data.promotionCustomer);
                data.promotionCustomer = await ConvertCustomerLocal(data.promotionCustomer);

                filterStore.filters.brand = data.promotionBrand;
                filterStore.filters.industry = data.promotionIndustry;

                ItemType.value = await getItemType(data.promotionBrand, data.promotionIndustry);

                data.fromDate = new Date(data.fromDate);
                data.toDate = new Date(data.toDate);

                data.promotionLine.forEach((el) => {
                    if (el.hasException) el.HasExceptionBlock = true;
                    el.promotionLineSub.forEach((sub) => {
                        if (sub.followBy == 1) sub.tanIndex = true;

                        sub.promotionItemBuyClone = cloneDeep(sub.promotionItemBuy);

                        if (sub.promotionItemBuy[0]?.itemType === 'G') {
                            const dataItems = ItemType.value.map((itemType) => ({
                                id: 0,
                                fatherId: 0,
                                itemType: 'G',
                                itemId: itemType.itemId,
                                itemCode: '',
                                itemName: itemType.itemName,
                                packing: itemType.packing,
                                status: ''
                            }));
                            sub.promotionItemBuy = dataItems.filter((e) => sub.promotionItemBuy.map((e) => e.itemId).includes(e.itemId));
                            sub.promotionUnitClone = cloneDeep(sub.promotionUnit);
                            sub.promotionUnit = sub.promotionUnit.map((e) => e.uomId);
                        }

                        sub.promotionLineSubSub.forEach((subSub) => {
                            subSub.promotionSubItemAddClone = cloneDeep(subSub.promotionSubItemAdd);
                        });
                    });
                    if (el.promotionLineSub.find((e) => e.followBy == 1) != undefined) {
                        el.promotionLineSub.find((e) => e.followBy == 1).tanIndex = false;
                    }
                });
                const { merge } = await import('lodash');
                payload.value = merge({}, payload.value, data);
            }
        } catch (error) {
            console.error(error);
        }
    };

    const getHierarchy = async () => {
        try {
            const res = await API.get('Item/hierarchy?cardId=');
            if (res.data.items) {
                Hierarchy.value = res.data.items;
            }
        } catch (error) {
            Hierarchy.value = [];
        }
    };

    // ==================== HIERARCHY / PRODUCT ====================
    const getDataIndustry = (dataId) => {
        if (!Hierarchy.value?.length) return [];
        const data = Hierarchy.value.filter((e) => dataId.includes(e.brandId));
        return uniqBy(
            data.flatMap((brand) => brand.industry),
            'industryId'
        );
    };

    const getItemType = async (promotionBrand, promotionIndustry) => {
        let dataItems = Hierarchy.value
            .filter((e) => promotionBrand.includes(e.brandId))
            .flatMap((e) => e.industry)
            .filter((e) => promotionIndustry.includes(e.industryId))
            .flatMap((e) => e.itemType);

        let mergedData = tanDev.values(tanDev.groupBy(dataItems, 'itemTypeId')).map((e) => mergedObj(e));

        mergedData = mergedData.map((obj) => {
            const { itemTypeName, itemTypeId, ...rest } = obj;
            return { itemName: itemTypeName, itemId: itemTypeId, ...rest };
        });

        return mergedData;
    };

    const mergedObj = (obj) => {
        const result = obj.reduce((acc, current) => {
            const existing = acc.find((item) => item.itemTypeId === current.itemTypeId);
            if (existing) {
                existing.packing = [...existing.packing, ...current.packing];
            } else {
                acc.push({ ...current });
            }
            return acc;
        }, []);
        return result[0];
    };

    const setUnitProduct = (data) => {
        return uniqBy(
            data.flatMap((item) => item.packing),
            'packingId'
        );
    };

    // ==================== FORM MANIPULATION ====================
    const addCondition = (data, subType) => {
        const promotionLineSub = cloneDeep(defaultPromotionLineSub);
        if (subType == 2) {
            if (data.filter((e) => e.status != 'D').length) {
                if (data.filter((e) => e.status != 'D').find((e) => e.followBy == 2) != undefined) promotionLineSub.followBy = 1;
                else promotionLineSub.followBy = 2;
            } else promotionLineSub.followBy = 1;
        } else promotionLineSub.followBy = 0;
        data.push(promotionLineSub);
    };

    const addRows = (data, index, cond = null, isSameType) => {
        const promotionLineSubSub = cloneDeep(defaultPromotionLineSubSub);
        promotionLineSubSub.inGroup = index;
        if (cond) promotionLineSubSub.cond = cond;
        data.push(promotionLineSubSub);
        if (isSameType) return;
        checkConditon(data);
    };

    const addForm = () => {
        const promotionLine = cloneDeep(defaultPromotionLine);
        const promotionLineSub = cloneDeep(defaultPromotionLineSub);
        if (payload.value.promotionLine.filter((e) => e.status != 'D' && !e.HasExceptionBlock)[0]?.subType == 1) {
            promotionLine.subType = 2;
            promotionLineSub.followBy = 1;
        } else {
            promotionLine.subType = 1;
        }
        payload.value.promotionLine.push(promotionLine);
        promotionLine.promotionLineSub.push(promotionLineSub);
    };

    const AddFormHasException = () => {
        const promotionLine = cloneDeep(defaultPromotionLine);
        const promotionLineSub = cloneDeep(defaultPromotionLineSub);
        promotionLine.status = 'A';
        if (payload.value.promotionLine.filter((e) => e.status != 'D' && e.HasExceptionBlock)[0].subType == 1) {
            promotionLine.subType = 2;
            promotionLineSub.followBy = 1;
        } else {
            promotionLine.subType = 1;
        }
        promotionLine.HasExceptionBlock = true;
        promotionLine.hasException = true;
        payload.value.promotionLine.push(promotionLine);
        promotionLine.promotionLineSub.push(promotionLineSub);
    };

    const AddConditionQuantity = (data, item) => {
        const sub = cloneDeep(item);
        sub.id = 0;
        sub.fatherId = 0;
        sub.status = 'A';
        sub.promotionItemBuy = [];
        sub.promotionUnit = [];
        sub.discountType = 'P';
        sub.priceType = 'P';
        sub.quantity = 0;
        data.promotionLineSub.push(sub);
        data.promotionLineSub = data.promotionLineSub.sort((a, b) => a.followBy - b.followBy);
    };

    const removeCondition = (data, item, index) => {
        if (item.id) {
            item.status = 'D';
            return;
        }
        data.promotionLineSub.splice(index, 1);
    };

    const removeConditionGroup = (data, item) => {
        if (item.id) {
            item.status = 'D';
            return;
        }
        item.indexGroup = true;
        data.promotionLineSub = data.promotionLineSub.filter((e) => !e.indexGroup);
    };

    const removeForm = (data, item) => {
        if (item.id) {
            item.status = 'D';
            return;
        }
        data.promotionLine = data.promotionLine.filter((e) => e != item);
    };

    const RemoveItem = (index) => {
        ItemData.value.splice(index, 1);
        for (let i = IntermediateVariable.value.length - 1; i >= 0; i--) {
            IntermediateVariable.value.splice(i, 1);
        }
        ItemData.value.forEach((el) => {
            IntermediateVariable.value.push({
                id: 0,
                fatherId: 0,
                itemType: 'I',
                itemId: el.id,
                itemCode: el.itemCode,
                itemName: el.itemName,
                status: ''
            });
        });
    };

    const RemoveItemProduct = (data, item) => {
        item.status = 'D';
        data.promotionLineSubSub = data.promotionLineSubSub.filter((e) => e.id || (e.status != 'D' && e.id == 0));
        if (data.isSameType) return;
        checkConditon(data.promotionLineSubSub);
    };

    // ==================== CLONE ====================
    const CloneCondition = (data, item) => {
        const itemClone = cloneDeep(item);
        itemClone.id = 0;
        itemClone.fatherId = 0;
        itemClone.status = 'A';
        itemClone.promotionItemBuyClone = [];
        itemClone.promotionUnitClone = [];
        itemClone.promotionItemBuy.forEach((el) => {
            el.fatherId = 0;
            el.id = 0;
            el.status = 'A';
        });

        if (itemClone.promotionLineSubSub?.length) {
            itemClone.promotionLineSubSub.forEach((el) => {
                el.id = 0;
                el.fatherId = 0;
                el.status = 'A';
                el.promotionSubItemAdd.forEach((e) => {
                    e.id = 0;
                    e.fatherId = 0;
                    e.status = 'A';
                });
                el.promotionSubItemAddClone = [];
                el.promotionSubUnitClone = [];
                el.promotionSubUnit.forEach((e) => {
                    e.id = 0;
                    e.fatherId = 0;
                    e.status = 'A';
                });
            });
        }
        data.promotionLineSub.push(itemClone);
        data.promotionLineSub = data.promotionLineSub.sort((a, b) => a.followBy - b.followBy);
    };

    const CloneConditionSub = (data, item) => {
        const itemClone = cloneDeep(item);
        itemClone.id = 0;
        itemClone.fatherId = 0;
        itemClone.status = 'A';
        itemClone.promotionSubItemAddClone = [];
        itemClone.promotionSubUnitClone = [];
        itemClone.promotionSubItemAdd.forEach((el) => {
            el.id = 0;
            el.fatherId = 0;
            el.status = 'A';
        });
        itemClone.promotionSubUnit.forEach((el) => {
            el.id = 0;
            el.fatherId = 0;
            el.status = 'A';
        });
        data.promotionLineSubSub.push(itemClone);
    };

    // ==================== CONDITION LOGIC ====================
    const checkConditon = (data) => {
        const uniqByinGroup = uniqBy(data, 'inGroup');
        if (uniqByinGroup.length < 2) {
            data[0].cond = 'AND';
            data.forEach((el) => {
                el.cond = 'AND';
            });
        }
        if (uniqByinGroup.length === 2) {
            uniqByinGroup.forEach((e) => {
                e.cond = uniqByinGroup[0].cond;
            });
        }
        if (uniqByinGroup.length > 2) {
            if (uniqByinGroup.filter((e) => e.cond === 'OR').length === 1) {
                const dataInGroup = data.find((e) => e.cond == 'AND' && e.inGroup != data[0].inGroup);
                if (dataInGroup != undefined) {
                    data.filter((e) => e.inGroup == dataInGroup.inGroup).forEach((e) => {
                        e.cond = 'OR';
                    });
                }
            }
        }
    };

    const changeCondition = (item, data, event, isSameType) => {
        item.forEach((el) => {
            el.cond = event.value;
        });
        if (isSameType) return;
        const setCond = (data, cond) => {
            data.forEach((e) => {
                e.cond = cond;
            });
        };

        const uniqByinGroup = uniqBy(data, 'inGroup');
        if (uniqByinGroup.length < 2) {
            uniqByinGroup.forEach((e) => {
                e.cond = 'AND';
            });
        }
        if (uniqByinGroup.length === 2) {
            uniqByinGroup.forEach((e) => {
                e.cond = event.value;
            });
        }
        if (uniqByinGroup.length > 2) {
            if (uniqByinGroup.filter((e) => e.cond === 'OR').length === 1) {
                const dataInGroup = data.find((e) => e.cond == 'AND' && e.inGroup != item[0].inGroup);
                if (dataInGroup != undefined) {
                    setCond(
                        data.filter((e) => e.inGroup == dataInGroup.inGroup),
                        'OR'
                    );
                }
            }
        }
    };

    const changConditonTow = (data, event) => {
        if (!event) {
            data.forEach((el) => {
                el.cond = 'AND';
            });
        }
    };

    const checkSubType = (id) => {
        if (payload.value.promotionLine.filter((e) => e.status != 'D' && !e.HasExceptionBlock).length > 1) {
            return [id];
        }
        return [1, 2];
    };

    const checkSubTypeHasException = (id) => {
        if (payload.value.promotionLine.filter((e) => e.status != 'D' && e.HasExceptionBlock).length > 1) {
            return [id];
        }
        return [1, 2];
    };

    const checkByIndex = (arr, subType) => {
        if (subType == 1) return true;
        const filtered = arr.filter((e) => e.status != 'D');
        const hasByID1 = filtered.some((item) => item.followBy === 1);
        const hasByID2 = filtered.some((item) => item.followBy === 2);
        return hasByID1 && hasByID2;
    };

    const getFirstIndex = (data) => {
        data = data.filter((e) => e.status !== 'D');
        const index = data.findIndex((item) => item.followBy === 1);
        return index;
    };

    const findMaxIngroup = (data) => {
        return Math.max(...data.map((obj) => obj.inGroup));
    };

    const ChangeSubFrom = (data, subType) => {
        if (subType == 2) {
            data[0].followBy = 1;
            data.forEach((e) => {
                if (!e.followBy) e.followBy = 1;
            });
        } else {
            data.forEach((e) => {
                e.followBy = 0;
            });
        }
    };

    const changeFormGroup = (followBy, data) => {
        data.forEach((el) => {
            el.followBy = followBy;
        });
    };

    const changeHasEx = (event) => {
        if (event) {
            if (!payload.value.promotionLine.some((e) => e.hasException)) {
                const promotionLineException = cloneDeep(defaultPromotionLine);
                promotionLineException.HasExceptionBlock = true;
                promotionLineException.hasException = true;
                const promotionLineSubException = cloneDeep(defaultPromotionLineSub);
                promotionLineException.promotionLineSub.push(promotionLineSubException);
                payload.value.promotionLine.push(promotionLineException);
            }
        } else {
            payload.value.promotionLine.array.forEach((e, index) => {
                if (e.hasException) {
                    if (e.id) {
                        e.status = 'D';
                    } else {
                        payload.value.promotionLine.splice(index, 1);
                    }
                }
            });
        }
    };

    const renderLabel = (data) => {
        if (!data.length) return '';
        const type = data[0].itemType == 'G' ? t('body.systemSetting.product_types') : t('body.systemSetting.products');
        const text = t('body.productManagement.selected_label') + ` ${data.length} ${type}`;
        return text;
    };

    // ==================== PRODUCT SELECTION ====================
    const IntermediateVariable = ref();

    const updateSelectedProduct = (data) => {
        if (!ItemData.value.length) {
            data.forEach((el) => {
                ItemData.value.push({
                    id: 0,
                    fatherId: 0,
                    itemType: 'I',
                    itemId: el.id,
                    itemCode: el.itemCode,
                    itemName: el.itemName,
                    status: 'A'
                });
            });
        } else {
            data.forEach((el) => {
                if (ItemData.value.find((e) => e.itemId == el.id) == undefined) {
                    ItemData.value.push({
                        id: 0,
                        fatherId: 0,
                        itemType: 'I',
                        itemId: el.id,
                        itemCode: el.itemCode,
                        itemName: el.itemName,
                        status: 'A'
                    });
                }
            });
        }
    };

    const confirmDataProduct = (data) => {
        for (let i = IntermediateVariable.value.length - 1; i >= 0; i--) {
            IntermediateVariable.value.splice(i, 1);
        }
        selectedItemType.value = [];
        data.forEach((el) => {
            IntermediateVariable.value.push(el);
        });
        visible.value = false;
    };

    const confirmDataTypeItem = (data) => {
        for (let i = IntermediateVariable.value.length - 1; i >= 0; i--) {
            IntermediateVariable.value.splice(i, 1);
        }
        ItemData.value = [];
        data.forEach((el) => {
            IntermediateVariable.value.push({
                id: 0,
                fatherId: 0,
                itemType: 'G',
                itemId: el.itemId,
                itemCode: '',
                itemName: el.itemName,
                packing: el.packing,
                status: 'A'
            });
        });
        visible.value = false;
    };

    const openSelectProduct = async (data, isFilter = false) => {
        visible.value = true;
        ItemData.value = [];
        IntermediateVariable.value = [];
        selectedItemType.value = [];
        IntermediateVariable.value = data;
        if (data[0]?.itemType === 'G') {
            const { promotionBrand, promotionIndustry } = payload.value;
            let dataItems = await getItemType(promotionBrand, promotionIndustry);
            selectedItemType.value = dataItems.filter((e) => data.flatMap((e) => e.itemId).includes(e.itemId));
        } else {
            ItemData.value = [...data];
        }
        isFilterPro.value = isFilter;
    };

    // ==================== CONVERSION ====================
    const ConvertIndustry = (id) => {
        const data = Hierarchy.value.flatMap((e) => e.industry).find((e) => e.industryId === id);
        return {
            id: 0,
            fatherId: 0,
            industryId: data.industryId,
            industryName: data.industryName,
            status: 'A'
        };
    };

    const ConvertBrand = (id) => {
        const data = Hierarchy.value.find((e) => e.brandId == id);
        return {
            id: 0,
            fatherId: 0,
            brandId: data.brandId,
            brandName: data.brandName,
            status: 'A'
        };
    };

    const ConvertCustomer = (data) => {
        if (data?.length) {
            data = data[0];
        }
        if (data?.items?.length) {
            return data?.items.map((el) => ({
                id: 0,
                fatherId: 0,
                type: data.type,
                customerId: el.id,
                CustomerCode: el.cardCode || '',
                customerName: el.cardName || el.groupName,
                status: 'A'
            }));
        }
        return [];
    };

    const ConvertCustomerLocal = async (data) => {
        if (Array.isArray(data) && data.length > 0) {
            const customer = [
                {
                    type: data[0]?.type || '',
                    items: data.map((e) => ({
                        id: e.customerId,
                        cardCode: e.CustomerCode,
                        cardName: e.customerName
                    }))
                }
            ];
            return customer;
        }
        return [{ type: '', items: [] }];
    };

    const convertData = (data, dataClone, dataTarget, id) => {
        const templateData = data[dataClone].flatMap((e) => e[id]);
        templateData.forEach((element) => {
            const lineCus = data[dataTarget].find((e) => e[id] === element);
            if (lineCus != undefined) {
                const dataUpdate = data[dataClone].find((e) => e[id] === element);
                lineCus.status = 'U';
                lineCus.fatherId = dataUpdate.fatherId;
                lineCus.id = dataUpdate.id;
            } else {
                const dataDelete = data[dataClone].find((e) => e[id] === element);
                if (dataDelete) {
                    dataDelete.status = 'D';
                    data[dataTarget].push(dataDelete);
                }
            }
        });
        delete data[dataClone];
    };

    // ==================== BRAND / INDUSTRY ====================
    const changerBrand = async () => {
        filterStore.filters.brand = payload.value.promotionBrand;
        payload.value.promotionIndustry = payload.value.promotionIndustry.filter((e) => getDataIndustry(payload.value.promotionBrand).find((el) => el.industryId == e));
        if (!payload.value.promotionIndustry.length) {
            ItemType.value = [];
        } else {
            ItemType.value = await getItemType(payload.value.promotionBrand, payload.value.promotionIndustry);
        }
    };

    const changerIndustry = async () => {
        filterStore.filters.industry = payload.value.promotionIndustry;
        ItemType.value = await getItemType(payload.value.promotionBrand, payload.value.promotionIndustry);
    };

    // ==================== VALIDATION ====================
    const validate = () => {
        let status = false;
        if (!payload.value.promotionName) {
            FunctionGlobal.$notify('E', 'Vui lòng nhập tên chương trình KM', toast);
            status = true;
        }
        if (!payload.value.fromDate) {
            FunctionGlobal.$notify('E', 'Vui lòng nhập ngày bắt đầu', toast);
            status = true;
        }
        if (!payload.value.toDate) {
            FunctionGlobal.$notify('E', 'Vui lòng nhập ngày kết thúc', toast);
            status = true;
        }

        let dataCus = {};
        if (payload.value.promotionCustomer?.length) {
            dataCus = payload.value.promotionCustomer[0];
        } else {
            dataCus = payload.value.promotionCustomer;
        }

        if (!payload.value.isAllCustomer && !dataCus?.items?.length) {
            FunctionGlobal.$notify('E', 'Vui lòng chọn khách hàng', toast);
            status = true;
        }
        if (!payload.value.promotionBrand.length) {
            FunctionGlobal.$notify('E', 'Vui lòng chọn thương hiệu', toast);
            status = true;
        }
        if (!payload.value.promotionIndustry.length) {
            FunctionGlobal.$notify('E', 'Vui lòng chọn ngành hàng', toast);
            status = true;
        }

        if (payload.value.promotionLine.length > 0)
            for (const promotionLine of payload.value.promotionLine) {
                for (const promotionLineSub of promotionLine.promotionLineSub) {
                    if (promotionLineSub.promotionItemBuy.length == 0) {
                        FunctionGlobal.$notify('E', 'Vui lòng thêm sản phẩm mua.', toast);
                        status = true;
                    }
                    if (promotionLineSub.promotionLineSubSub.length > 0) {
                        for (const promotionLineSubSub of promotionLineSub.promotionLineSubSub) {
                            if (promotionLineSubSub.promotionSubItemAdd.length == 0) {
                                FunctionGlobal.$notify('E', 'Vui lòng thêm sản phẩm tặng.', toast);
                                status = true;
                            }
                        }
                    }
                }

                if (promotionLine.subType == 2) {
                    const subs = promotionLine.promotionLineSub.filter((e) => e.status != 'D');
                    const firstGroup2 = subs.find((e) => e.followBy == 2);
                    if (firstGroup2 && (!firstGroup2.fromDate || !firstGroup2.toDate)) {
                        FunctionGlobal.$notify('E', 'Vui lòng nhập thời gian tính sản lượng thực tế', toast);
                        status = true;
                    }
                }
            }

        return status;
    };

    // ==================== SAVE ====================
    const formatData = (d) => {
        const date = new Date(d);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${year}-${month}-${day}`;
    };

    const SavePromotion = async () => {
        submited.value = true;
        if (validate()) {
            return;
        }
        loading.value = true;

        const dataApi = cloneDeep(payload.value);
        if (!dataApi.hasException) {
            dataApi.promotionLine
                .filter((e) => e.hasException && e.id)
                .forEach((e) => {
                    e.status = 'D';
                });
            dataApi.promotionLine = dataApi.promotionLine.filter((e) => !e.hasException || (e.hasException && e.id));
        }
        dataApi.promotionBrand = dataApi.promotionBrand.map((el) => ConvertBrand(el));
        dataApi.promotionCustomer = ConvertCustomer(dataApi.promotionCustomer);
        if (dataApi.promotionCustomerClone) {
            if (dataApi.isAllCustomer) dataApi.promotionCustomer = [];
            convertData(dataApi, 'promotionCustomerClone', 'promotionCustomer', 'customerId');
        }
        if (dataApi.promotionBrandClone) {
            convertData(dataApi, 'promotionBrandClone', 'promotionBrand', 'brandId');
        }

        dataApi.promotionIndustry = dataApi.promotionIndustry.map((el) => {
            el = ConvertIndustry(el);
            return el;
        });
        if (dataApi.promotionIndustryClone) {
            convertData(dataApi, 'promotionIndustryClone', 'promotionIndustry', 'industryId');
        }
        let err = false;
        dataApi.promotionLine.forEach((line) => {
            line.status = !line.id ? (line.status = 'A') : !line.status ? 'U' : line.status;
            line.promotionLineSub.forEach((subLine) => {
                subLine.status = subLine.id && !subLine.status ? 'U' : subLine.status;
                subLine.status = !subLine.id ? (subLine.status = 'A') : !subLine.status ? 'U' : subLine.status;
                if (subLine.promotionItemBuy[0]?.itemType == 'G') {
                    if (subLine.promotionUnit.length == 0) {
                        FunctionGlobal.$notify('E', 'Vui lòng chọn quy cách bao bì', toast);
                        err = true;
                    }
                    subLine.promotionUnit = setUnitProduct(subLine.promotionItemBuy)
                        .map((item) => {
                            return {
                                id: 0,
                                fatherId: 0,
                                uomId: item.packingId,
                                uomName: item.packingName,
                                status: 'A'
                            };
                        })
                        .filter((e) => subLine.promotionUnit.includes(e.uomId));
                } else {
                    subLine.promotionUnit = [];
                }

                if (subLine.promotionUnitClone && subLine.promotionItemBuy[0]?.itemType == 'G') {
                    convertData(subLine, 'promotionUnitClone', 'promotionUnit', 'uomId');
                }
                if (subLine.promotionItemBuyClone) {
                    convertData(subLine, 'promotionItemBuyClone', 'promotionItemBuy', 'itemId');
                }
                subLine.promotionLineSubSub.forEach((subSubLine) => {
                    subSubLine.status = !subSubLine.id ? (subSubLine.status = 'A') : !subSubLine.status ? 'U' : subSubLine.status;
                    if (subSubLine.promotionSubItemAddClone) {
                        convertData(subSubLine, 'promotionSubItemAddClone', 'promotionSubItemAdd', 'itemId');
                    }
                });
            });
        });

        if (err) {
            loading.value = false;
            return;
        }
        dataApi.fromDate = formatData(dataApi.fromDate);
        dataApi.toDate = formatData(dataApi.toDate);
        dataApi.promotionLine.forEach((el) => {
            if (el.subType == 2) {
                const fromDateSub = el.promotionLineSub.find((e) => e.followBy == 2 && e.fromDate);
                const toDateSub = el.promotionLineSub.find((e) => e.followBy == 2 && e.toDate);
                el.promotionLineSub.forEach((sub) => {
                    if (sub.followBy == 2) {
                        sub.fromDate = fromDateSub != undefined ? formatData(fromDateSub.fromDate) : null;
                        sub.toDate = toDateSub != undefined ? formatData(toDateSub.toDate) : null;
                    }
                });
            }
        });
        try {
            const FUNCAPI = dataApi.id ? await API.update(`promotion/${dataApi.id}`, dataApi) : await API.add('promotion/add', dataApi);
            const res = FUNCAPI;
            if (res) {
                FunctionGlobal.$notify('S', dataApi.id ? t('client.update_success') : t('PromotionalItems.SetupPurchases.addnewSuccess'), toast);
                router.push(`/promotion`);
            }
        } catch (error) {
            if (error.status == 403) {
                FunctionGlobal.$notify('E', t('PromotionalItems.SetupPurchases.youDontAction'), toast);
            } else {
                FunctionGlobal.$notify('E', error.response.data.errors || 'Đã có lỗi xảy ra', toast);
            }
        } finally {
            loading.value = false;
        }
    };

    // ==================== AI ====================
    const generateAI = async () => {
        if (!payload.value.promotionName) return;
        const res = await fetch('https://api.openai.com/v1/chat/completions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${import.meta.env.VITE_APP_OPENAI_KEY}`
            },
            body: JSON.stringify({
                model: 'gpt-4.1-mini',
                messages: [
                    {
                        role: 'user',
                        content: `Hãy đề xuất 1 tên khuyến mãi mới, hấp dẫn hơn, dựa trên tên khuyến mãi sau:"${payload.value.promotionName}",
                            Yêu cầu:
                            - Chỉ trả về 1 tên khuyến mãi 
                            - Tiếng Việt
                            - Không emoji
                            - Chuẩn SEO, hấp dẫn
                            - Không giải thích không quá 50 kí tự`
                    }
                ]
            })
        });
        const data = await res.json();
        payload.value.promotionName = data.choices[0].message.content;
    };

    return {
        // State
        payload,
        response,
        loading,
        submited,
        visible,
        isFilterPro,
        ItemData,
        ItemType,
        selectedItemType,
        Hierarchy,
        filterStore,
        // Options
        optionAddValueType,
        optionAddType,
        optionFollowBy,
        optionFollowBy2,
        optionSubType,
        optionPromotionType,
        comparisonOptions,
        // Router
        router,
        route,
        // Functions
        format,
        groupBy,
        getDataIndustry,
        setUnitProduct,
        addCondition,
        addRows,
        addForm,
        AddFormHasException,
        AddConditionQuantity,
        removeCondition,
        removeConditionGroup,
        removeForm,
        RemoveItem,
        RemoveItemProduct,
        CloneCondition,
        CloneConditionSub,
        changeCondition,
        changConditonTow,
        checkSubType,
        checkSubTypeHasException,
        checkByIndex,
        getFirstIndex,
        findMaxIngroup,
        ChangeSubFrom,
        changeFormGroup,
        changeHasEx,
        renderLabel,
        changerBrand,
        changerIndustry,
        openSelectProduct,
        updateSelectedProduct,
        confirmDataProduct,
        confirmDataTypeItem,
        SavePromotion,
        generateAI,
        t
    };
}
