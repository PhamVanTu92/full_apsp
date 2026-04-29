import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import API from '../api/api-main';

export const useHierarchyStore = defineStore('hierarchy', () => {
    const hierarchies = ref([]);
    // State cho filters
    const filters = ref({
        search: '',
        brand: [],
        industry: [],
        itemtype: [],
        packing: []
    });

    const loadHierarchies = async (cardId) => {  
        if (hierarchies.value.length < 1) {
            try { 
                const res = await API.get('item/hierarchy?cardId=' + (cardId || ''));
                hierarchies.value = res.data?.items || [];
            } catch (err) {
                // Error loading hierarchy
            }
        }
    }; 

    const getBrandOptions = computed(() => {
        return hierarchies.value;
    });

    /**
     * @param { Array<Int> }  brandIds mảng chứa các id của thương hiệu
     * @returns { Array<Object> } mảng chứa options ngành hàng nằm trong thương hiệu
     */
    const getIndustryOptions = (brandIds = [], queue = true) => { 
        if (brandIds?.length < 1) return [];
        let selectedBrands = hierarchies.value.filter((brand) => {
            return brandIds.includes(brand.brandId);
        });

        let unqueueIndustries = selectedBrands.reduce((accumulator, currentValue) => {
            return accumulator.concat(currentValue.industry);
        }, []);

        if (queue) {
            let dataQueue = unqueueIndustries.filter((item, index, self) => index === self.findIndex((t) => t.industryId === item.industryId));
            return dataQueue;
        } else return unqueueIndustries;
    };

    /**
     * @param { Array<Int> }  brandIds - mảng chứa các id của thương hiệu
     * @param { Array<Int> }  industryIds - mảng chứa các id của các ngành hàng
     * @returns { Array<Object> } - mảng chứa options các loại sản phẩm nằm trong thương hiệu và ngành hàng
     *
     */
    const getItemTypeOptions = (brandIds = [], industryIds = [], queue = true) => { 
        if (brandIds?.length < 1 || industryIds?.length < 1) return [];
        let unqueueIndustries = getIndustryOptions(brandIds, false);
        let selectedIndustryIds = unqueueIndustries.filter((industry) => {
            return industryIds.includes(industry.industryId);
        });
        let unqueueItemType = selectedIndustryIds.reduce((accumulator, currentValue) => {
            return accumulator.concat(currentValue.itemType);
        }, []);

        if (queue) {
            let dataQueue = unqueueItemType.filter((item, index, self) => index === self.findIndex((t) => t.itemTypeId === item.itemTypeId));
            return dataQueue;
        } else {
            return unqueueItemType;
        }
    };

    /**
     * @param { Array<Int> }  brandIds - mảng chứa các id của thương hiệu
     * @param { Array<Int> }  industryIds - mảng chứa các id của các ngành hàng
     * @returns { Array<Object> } - mảng chứa options các loại sản phẩm nằm trong thương hiệu và ngành hàng
     *
     */
    const getPackingOptions = (brandIds = [], industryIds = [], itemTypeIds = [], queue = true) => {
        let unqueueItemType = getItemTypeOptions(brandIds, industryIds, false);
        let selectedItemTypes = unqueueItemType.filter((itemType) => {
            return itemTypeIds.includes(itemType.itemTypeId);
        });
        let unqueuePacking = selectedItemTypes.reduce((accumulator, currentValue) => {
            return accumulator.concat(currentValue.packing);
        }, []);
        if (queue) {
            let dataQueue = unqueuePacking.filter((item, index, self) => index === self.findIndex((t) => t.packingId === item.packingId));
            return dataQueue;
        } else {
            return unqueuePacking;
        }
    };

    // Actions cho filters
    const setSearchFilter = (searchValue) => {
        filters.value.search = searchValue;
    };

    const setBrandFilter = (brands) => {
        filters.value.brand = brands;
    };

    const setIndustryFilter = (industries) => {
        filters.value.industry = industries;
    };

    const setItemTypeFilter = (itemTypes) => {
        filters.value.itemtype = itemTypes;
    };

    const setPackingFilter = (packings) => {
        filters.value.packing = packings;
    };

    const getSearchFilter = computed(() => filters.value.search);

    const getBrandFilter = computed(() => filters.value.brand);

    const getIndustryFilter = computed(() => filters.value.industry);

    const getItemTypeFilter = computed(() => filters.value.itemtype);

    const getPackingFilter = computed(() => filters.value.packing);

    const resetStore = () => {
        filters.value.search = '';
        filters.value.brand = [];
        filters.value.industry = [];
        filters.value.itemtype = [];
        filters.value.packing = [];
    }

    return { hierarchies, loadHierarchies, getBrandOptions, getIndustryOptions, getItemTypeOptions, getPackingOptions, filters, setSearchFilter, setBrandFilter, setIndustryFilter, setItemTypeFilter, setPackingFilter, getSearchFilter, getBrandFilter, getIndustryFilter, getItemTypeFilter, getPackingFilter, resetStore };
});
