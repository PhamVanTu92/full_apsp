<template>
    <div>
        <div class="mb-3 flex justify-content-between">
            <IconField iconPosition="left">
                <InputIcon class="pi pi-search"> </InputIcon>
                <InputText v-model="selectionStore.search" :placeholder="t('body.home.search_placeholder')" class="w-30rem"></InputText>
            </IconField>
            <div>
                <span class="mr-3">{{ t('client.sort_by') }}:</span>
                <Dropdown v-model="sort" :placeholder="t('client.select')" :options="options.sort" optionValue="value" optionLabel="label" class="w-12rem" @change="fetchData(selectionStore.getQueryParam)" />
            </div>
        </div>
        <div class="card py-2 px-3 mb-3 justify-content-between gap-2 flex align-items-center surface-200">
            <span class="font-bold py-2 white-space-nowrap">{{ t('client.filter') }} </span>
            <div class="flex-grow-1 white-space-nowrap overflow-hidden overflow-x-scroll scroll-bar-hidden">
                <div class="w-full flex gap-1">
                    <template v-for="(item, index) in selectionStore.getChips(brandStore.data, itemTypeStore.data)" :key="index">
                        <Chip class="border-1 surface-0" :class="getClass(item.tag)" v-tooltip="{ value: item.label || item.id, showDelay: 1000 }">
                            <div class="flex-grow-1 max-w-10rem overflow-hidden text-overflow-ellipsis white-space-nowrap">
                                {{ item.label || item.id }}
                            </div>
                            <div class="flex ml-1">
                                <i class="pi pi-times-circle my-auto cursor-pointer" @click="onRemoveChip(item)"></i>
                            </div>
                        </Chip>
                    </template>
                </div>
            </div>
            <span class="py-2 white-space-nowrap">
                <span class="font-bold">{{ product.total }}</span>
                {{ t('client.products') }}
            </span>
        </div>
        <div class="grid">
            <template v-if="product.data.length">
                <div v-for="(item, i) in product.data" :key="i" class="col-3">
                    <!-- ['id', 'name', 'image', 'price', 'oldPrice', 'status', 'isDetail', 'note', 'rating', 'class'] -->
                    <ProductCard :id="item.id" :currency="item.currency" :name="item.itemName" :price="item.price" :image="item.itM1[0]?.filePath" class="h-full" />
                </div>
            </template>
            <div v-else class="col-12">
                <div class="text-center card">
                    <div class="my-8 py-8 text-500 font-italic">Không tìm thấy kết quả phù hợp</div>
                </div>
            </div>
        </div>
        <Paginator class="card p-0" :rows="selectionStore.limit" :totalRecords="product.total" :rowsPerPageOptions="[12, 24, 36]" @page="onPagging" :first="selectionStore.limit * selectionStore.skip"> </Paginator>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue';
import API from '@/api/api-main';
import { useBrandStore } from '@/Pinia/brand';
import { useItemTypeStore } from '@/Pinia/itemType';
import { useSelectionFilterStore } from '@/Pinia/productFilter';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const brandStore = useBrandStore();
const itemTypeStore = useItemTypeStore();
const selectionStore = useSelectionFilterStore();
const PATH = 'Item';

const loading = ref(false);
const product = reactive({
    data: [] as Array<any>,
    total: 0,
    skip: 0,
    limit: 0
});

const onRemoveChip = (chip: any) => {
    selectionStore.removeChip(chip.id, chip.tag);
};

const classes = {
    itemtype: 'border-green-300',
    brand: 'border-orange-300'
};
const getClass = (tag: 'itemtype' | 'brand'): string => {
    return classes[tag];
};

const onPagging = (event: { page: number; rows: number }) => {
    selectionStore.skip = event.page;
    selectionStore.limit = event.rows;
};

const fetchData = (query: string) => {
    const URI = `${`${PATH}/bypass` + query + `&OrderBy=${sort.value}` + `&filter=(itemTypeId != 16)`}`;

    loading.value = true;
    API.get(URI)
        .then((res) => {
            product.data = res.data.items;
            product.limit = res.data.limit;
            product.total = res.data.total;
            product.skip = res.data.skip;
        })
        .catch((error) => {
            console.error(error);
        })
        .finally(() => {
            loading.value = false;
        });
};

const sort = ref('price asc');
const options = computed(() => ({
    sort: [
        { value: 'price asc', label: t('client.price_asc') },
        { value: 'price desc', label: t('client.price_desc') }
        // { value: 'itemName asc', label: 'Sắp xếp A → Z' },
        // { value: 'itemName desc', label: 'Sắp xếp Z → A' }
    ]
}));

watch(
    () => selectionStore.getQueryParam,
    (_query) => {
        fetchData(_query);
    }
);

const initialComponent = () => {
    // code here
    fetchData(selectionStore.getQueryParam);
};

onMounted(function () {
    initialComponent();
    if (!brandStore.data.length) brandStore.fetchAll();
    if (!itemTypeStore.data.length) itemTypeStore.fetchAll();
});

onUnmounted(function () {
    selectionStore.$dispose();
});
</script>

<style scoped>
.scroll-bar-hidden::-webkit-scrollbar {
    display: none;
}
</style>
