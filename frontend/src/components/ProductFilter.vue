<template>
  <div
    class="flex w-full gap-2 h-auto mr-1 align-items-center overflow-hidden overflow-x-auto"
  >
    <div :class="screenW < 768 ? 'w-7rem' : 'w-4'">
      <IconField iconPosition="left" :class="screenW < 768 ? 'w-7rem' : 'w-full'">
        <InputIcon class="pi pi-search"></InputIcon>
        <InputText
          v-model="filters.search"
          :class="screenW < 768 ? 'w-7rem' : 'w-full'"
          :placeholder="t('body.productManagement.search_placeholder')"
          @keyup="debouncer()"
        />
      </IconField>
    </div>
    <MultiSelect
      :class="screenW < 768 ? 'w-7rem' : 'w-2'"
      selectedItemsLabel="{0} điều kiện lọc"
      :disabled="props.disabledHeader"
      :maxSelectedLabels="3"
      v-model="filters.brand"
      :options="layers.Brands"
      optionLabel="brandName"
      optionValue="brandId"
      :placeholder="t('body.productManagement.brand')"
      filter
      @change="onChangeBrand"
      v-if="props.andPointApi !== 'Item/ItemPromotion'"
    />
    <MultiSelect
      :class="screenW < 768 ? 'w-7rem' : 'w-2'"
      :disabled="filters.brand.length < 1 || props.disabledHeader"
      selectedItemsLabel="{0} điều kiện lọc"
      :maxSelectedLabels="3"
      optionValue="industryId"
      v-model="filters.industry"
      :options="layers.Industries"
      optionLabel="industryName"
      :placeholder="t('body.productManagement.category')"
      filter
      @change="onChangeIndustry"
      v-if="props.andPointApi !== 'Item/ItemPromotion'"
    />
    <MultiSelect
      :class="screenW < 768 ? 'w-7rem' : 'w-2'"
      :disabled="filters.industry.length < 1"
      selectedItemsLabel="{0} điều kiện lọc"
      :maxSelectedLabels="3"
      optionValue="itemTypeId"
      v-model="filters.itemtype"
      :options="layers.ItemTypes"
      optionLabel="itemTypeName"
      :placeholder="t('body.productManagement.product_type')"
      filter
      @change="onChangeItemType"
      v-if="props.andPointApi !== 'Item/ItemPromotion'"
    />
    <MultiSelect
      :class="screenW < 768 ? 'w-7rem' : 'w-2'"
      :disabled="filters.itemtype < 1"
      selectedItemsLabel="{0} điều kiện lọc"
      :maxSelectedLabels="3"
      optionValue="packingId"
      v-model="filters.packing"
      :options="layers.Packages"
      optionLabel="packingName"
      filter
      :placeholder="t('body.productManagement.packaging')"
      @change="onChangePack"
      v-if="props.andPointApi !== 'Item/ItemPromotion'"
    />
    <template v-if="props.clearable">
      <div class="flex">
        <slot v-if="slots.clear" name="clear" v-bind="{ clearFilter }"></slot>
        <Button
          v-else
          :label="t('body.productManagement.clear_button')"
          icon="pi pi-filter-slash"
          severity="danger"
          class="w-12"
          outlined
          @click="clearFilter()"
          :disabled="props.disabledHeader"
        />
      </div>
    </template>
  </div>
</template>

<script setup>
  import { ref, onMounted, watch, useSlots, watchEffect } from "vue";
  import { debounce } from "lodash";
  import { FilterStore } from "@/Pinia/FilterPromotion";
  import { useI18n } from "vue-i18n";

  const { t } = useI18n();
  const filterStore = FilterStore();
  const emits = defineEmits(["change"]);
  const slots = useSlots();

  const screenW = ref(window.innerWidth);
  window.addEventListener("resize", () => {
    screenW.value = window.innerWidth;
  });

  const props = defineProps({
    clearable: {
      default: false,
      type: Boolean,
    },
    isFilterPro: {
      type: Boolean,
      default: false,
    },
    disabledHeader: {
      type: Boolean,
      default: false,
    },
    cardId: {
      type: Number,
      default: 0,
    },
    andPointApi: {
      type: String,
      default: "",
    },
  });

  const data = ref([]);

  const filters = ref({
    search: "",
    brand: [],
    industry: [],
    itemtype: [],
    packing: [],
  });

  const layers = ref({
    Brands: [],
    Industries: [],
    ItemTypes: [],
    Packages: [],
  });

  const clearFilter = () => {
    let df = {
      search: "",
      brand: [],
      industry: [],
      itemtype: [],
      packing: [],
    };
    Object.assign(filters.value, df);
    hierarchyStore.setBrandFilter([]);
    hierarchyStore.setIndustryFilter([]);
    hierarchyStore.setItemTypeFilter([]);
    hierarchyStore.setPackingFilter([]);
    debouncer();
  };

  const IndustriesUnqueue = ref([]);
  const onChangeBrand = (event) => {
    let brands = data.value.filter((brand) => {
      return filters.value.brand.includes(brand.brandId);
    });

    IndustriesUnqueue.value = brands.reduce((accumulator, currentValue) => {
      return accumulator.concat(currentValue.industry);
    }, []);

    layers.value.Industries = IndustriesUnqueue.value.filter(
      (item, index, self) =>
        index === self.findIndex((t) => t.industryId === item.industryId)
    );
    layers.value.Industries.map((val) => {
      if (filters.value.industry.filter((ind) => ind === val.industryId).length !== 0) {
        val.itemType.map((valt) => {
          layers.value.ItemTypes.push(valt);
        });
      }
    });
    ItemTypeUnqueue.value = layers.value.ItemTypes;
    hierarchyStore.setBrandFilter(filters.value.brand);
    debouncer();
  };

  const ItemTypeUnqueue = ref([]);
  const onChangeIndustry = (event) => {
    let industries = IndustriesUnqueue.value.filter((industry) => {
      return filters.value.industry.includes(industry.industryId);
    });

    ItemTypeUnqueue.value = industries.reduce((accumulator, currentValue) => {
      return accumulator.concat(currentValue.itemType);
    }, []);

    layers.value.ItemTypes = ItemTypeUnqueue.value.filter(
      (item, index, self) =>
        index === self.findIndex((t) => t.itemTypeId === item.itemTypeId)
    );
    hierarchyStore.setIndustryFilter(filters.value.industry);
    debouncer();
  };

  const onChangeItemType = () => {
    const selectedItemTypes = layers.value.Brands.filter((brand) =>
      filters.value.brand.includes(brand.brandId)
    )
      .flatMap((brand) => brand.industry)
      .filter((industry) => filters.value.industry.includes(industry.industryId))
      .flatMap((industry) => industry.itemType)
      .filter((itemType) => filters.value.itemtype.includes(itemType.itemTypeId));

    layers.value.Packages = selectedItemTypes
      .flatMap((itemType) => itemType.packing)
      .filter(
        (item, index, self) =>
          index === self.findIndex((t) => t.packingId === item.packingId)
      );

    hierarchyStore.setItemTypeFilter(filters.value.itemtype);
    debouncer();
  };

  const onChangePack = () => {
    hierarchyStore.setPackingFilter(filters.value.packing);
    debouncer();
  };

  const generateQuery = () => {
    let queries = [];
    let { search, ...arr } = filters.value;
    if (search.trim()) {
      queries.push(`search=${search}`);
    }
    Object.keys(arr).forEach((key) => {
      if (arr[key].length > 0) {
        queries.push(`${key}=,${arr[key].join(",")},`);
      }
    });
    let query = null;
    if (queries.length > 0) {
      query = "?" + queries.join("&");
    }
    if (firstInit) {
      firstInit = false;
      return;
    }
    emits("change", query);
  };
  var debouncer = debounce(generateQuery, 500);
  var firstInit = true;

  watchEffect(() => {
    if (filters.value) {
      debouncer();
    }
  });

  import { useHierarchyStore } from "../Pinia/hierarchyStore";
  const hierarchyStore = useHierarchyStore();
  onMounted(async () => {
    await hierarchyStore.loadHierarchies(props.cardId);
    data.value = hierarchyStore.hierarchies;
    hierarchyStore.getBrandFilter != null
      ? ((filters.value.brand = hierarchyStore.getBrandFilter), onChangeBrand())
      : "";
    hierarchyStore.getIndustryFilter != null
      ? ((filters.value.industry = hierarchyStore.getIndustryFilter), onChangeIndustry())
      : "";
    hierarchyStore.getItemTypeFilter != null
      ? ((filters.value.itemtype = hierarchyStore.getItemTypeFilter), onChangeItemType())
      : "";
    hierarchyStore.getPackingFilter != null
      ? (filters.value.packing = hierarchyStore.getPackingFilter)
      : "";
    layers.value.Brands = data.value;
    if (props.isFilterPro) {
      filters.value = filterStore.filters;
      onChangeBrand();
      generateQuery();
    } else {
      // clearFilter();
    }
  });

  watch(
    () => props.cardId,
    async (newVal, oldVal) => {
      if (newVal !== oldVal) await hierarchyStore.loadHierarchies(newVal);
    }
  );
</script>

<style></style>
