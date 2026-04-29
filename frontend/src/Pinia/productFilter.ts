import { defineStore } from 'pinia';
import { computed, ref, watchEffect } from 'vue';
import { useRoute, useRouter } from 'vue-router';

interface Chip {
    label: string | any;
    tag: 'itemtype' | 'brand';
    id: number;
}

export const useSelectionFilterStore = defineStore('productFilter', () => {
    const route = useRoute();
    const router = useRouter();

    const itemType = ref<number[]>([]);
    const brand = ref<number[]>([]);
    const search = ref('');

    const limit = ref(12);
    const skip = ref(0);

    let timeOut: ReturnType<typeof setTimeout> | null = null;
    // initial();
    function initial() {
    if (route.name !== 'categories') return;

    if (typeof route.query.itemtype === 'string') {
        itemType.value = route.query.itemtype
            .split(',')
            .map((x) => Number(x))
            .filter((n) => !isNaN(n));
    }

    if (typeof route.query.brand === 'string') {
        brand.value = route.query.brand
            .split(',')
            .map((x) => Number(x))
            .filter((n) => !isNaN(n));
    }

    if (typeof route.query.skip === 'string') {
        skip.value = Number(route.query.skip);
    }

    if (typeof route.query.limit === 'string') {
        limit.value = Number(route.query.limit);
    }
}
    watchEffect(() => {
        if (route.name !== 'categories') return;

        const query: Record<string, string> = { ...route.query as Record<string, string> };
        if (itemType.value.length) {
            query['itemTypeId'] = itemType.value.join(',');
        } else delete query['itemTypeId'];
        if (brand.value.length) {
            query['brandId'] = brand.value.join(',');
        } else delete query['brandId'];
        if (search.value.trim()) {
            query['search'] = encodeURIComponent(search.value.trim());
        } else delete query['search'];
        query.skip = `${skip.value}`;
        query.limit = `${limit.value}`;

        if (timeOut) clearTimeout(timeOut);
        timeOut = setTimeout(() => {
            router.replace({ path: route.path, query });
        }, 300);
    });

    const getQueryParam = computed<string>(() => {
        const query: Array<string> = [`Page=${skip.value+1}&PageSize=${limit.value}`];

        if (itemType.value.length) {
            query.push(`itemTypeId=${itemType.value.join(',')}`);
        }
        if (brand.value.length) {
            query.push(`brandId=${brand.value.join(',')}`);
        }
        if (search.value.trim()) {
            query.push(`search=${encodeURIComponent(search.value.trim())}`);
        }
        return `?${query.join('&')}`;
    })

    function getChips(brands?: any[], itemTypes?: any[]): Array<Chip> {
        let result = [] as Array<Chip>;
        brand.value.forEach((id) => {
            result.push({
                label: brands?.find((itm: any) => itm.id == id)?.name,
                tag: 'brand',
                id: id
            });
        });
        itemType.value.forEach((id) => {
            result.push({
                label: itemTypes?.find((itm: any) => itm.id == id)?.name,
                tag: 'itemtype',
                id: id
            });
        });
        return result;
    }

    function removeChip(_id: number, tag: 'itemtype' | 'brand') {
        if (tag == 'itemtype') {
            itemType.value = itemType.value.filter((id: number) => id != _id);
        } else if (tag == 'brand') {
            brand.value = brand.value.filter((id: number) => id != _id);
        }
    }

    return {
        itemType,
        brand,
        limit,
        skip,
        search,
        getChips,
        removeChip,
        getQueryParam
    };
});
