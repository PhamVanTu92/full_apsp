<script setup>
import { ref, onBeforeMount, watch, computed } from "vue";
import { useRoute } from "vue-router";
import { useLayout } from "@/layout/composables/layout";

const { layoutConfig, layoutState, setActiveMenuItem, onMenuToggle } = useLayout();
const route = useRoute();
const isActiveMenu = ref(false);
const itemKey = ref(null);

const props = defineProps({
  item: {
    type: Object,
    default: () => ({}),
  },
  index: {
    type: Number,
    default: 0,
  },
  root: {
    type: Boolean,
    default: true,
  },
  parentItemKey: {
    type: String,
    default: null,
  },
  searchQuery: {
    type: String,
    default: ''
  }
});

// Hàm fuzzy match
const fuzzyMatch = (text, query) => {
  if (!text || !query) return false;
  const textLower = text.toLowerCase().replace(/\s+/g, ' ').trim();
  const queryLower = query.toLowerCase().replace(/\s+/g, ' ').trim();
  const queryWords = queryLower.split(' ').filter(word => word.length > 0);
  return queryWords.every(word => textLower.includes(word));
};

// Kiểm tra xem item có khớp với search query không
const matchesSearch = computed(() => {
  if (!props.searchQuery) return true;
  return fuzzyMatch(props.item.label, props.searchQuery);
});

// Kiểm tra xem có item con nào khớp không
const hasMatchingChildren = computed(() => {
  if (!props.searchQuery || !props.item.items) return false;
  
  const checkChildren = (items) => {
    return items.some(child => {
      if (child.visible === false) return false;
      if (fuzzyMatch(child.label, props.searchQuery)) {
        return true;
      }
      if (child.items) {
        return checkChildren(child.items);
      }
      return false;
    });
  };
  
  return checkChildren(props.item.items);
});

// Item có hiển thị không (dựa vào search)
const isVisible = computed(() => {
  if (!props.searchQuery) return true;
  return matchesSearch.value || hasMatchingChildren.value;
});

// Highlight text trong label - highlight tất cả các từ khớp
const highlightedLabel = computed(() => {
  if (!props.searchQuery || !matchesSearch.value) {
    return props.item.label;
  }
  
  let result = props.item.label;
  const queryWords = props.searchQuery
    .toLowerCase()
    .split(' ')
    .filter(word => word.length > 0);
  
  // Highlight từng từ trong query
  queryWords.forEach(word => {
    const regex = new RegExp(`(${word})`, 'gi');
    result = result.replace(regex, '<mark class="search-highlight">$1</mark>');
  });
  
  return result;
});

onBeforeMount(() => {
  itemKey.value = props.parentItemKey
    ? props.parentItemKey + "-" + props.index
    : String(props.index);

  const activeItem = layoutState.activeMenuItem;

  isActiveMenu.value =
    activeItem === itemKey.value || activeItem
      ? activeItem.startsWith(itemKey.value + "-")
      : false;
});

watch(
  () => layoutConfig.activeMenuItem.value,
  (newVal) => {
    isActiveMenu.value =
      newVal === itemKey.value || newVal.startsWith(itemKey.value + "-");
  }
);

// Tự động expand khi search và có kết quả
watch(
  () => props.searchQuery,
  (newVal) => {
    if (newVal && hasMatchingChildren.value && props.item.items) {
      isActiveMenu.value = true;
    } else if (!newVal) {
      // Reset về trạng thái ban đầu khi xóa search
      const activeItem = layoutState.activeMenuItem;
      isActiveMenu.value =
        activeItem === itemKey.value || activeItem
          ? activeItem.startsWith(itemKey.value + "-")
          : false;
    }
  },
  { immediate: true }
);

const itemClick = (event, item) => {
  if (item.disabled) {
    event.preventDefault();
    return;
  }

  const { overlayMenuActive, staticMenuMobileActive } = layoutState;

  if (
    (item.to || item.url) &&
    (staticMenuMobileActive.value || overlayMenuActive.value)
  ) {
    onMenuToggle();
  }

  if (item.command) {
    item.command({ originalEvent: event, item: item });
  }

  const foundItemKey = item.items
    ? isActiveMenu.value
      ? props.parentItemKey
      : itemKey
    : itemKey.value;

  setActiveMenuItem(foundItemKey);
};

const checkActiveRoute = (item) => {
  return route.path === item.to;
};
</script>

<template>
  <li 
    v-if="isVisible"
    :class="{ 
      'layout-root-menuitem': root, 
      'active-menuitem': isActiveMenu,
      'search-match': matchesSearch && searchQuery 
    }"
  >
    <div
      v-if="root && item.visible !== false"
      class="layout-menuitem-root-text text-sm text-gray-500"
    >
      {{ item.label }}
    </div>
    <a
      v-if="(!item.to || item.items) && item.visible !== false"
      :href="item.url"
      @click="itemClick($event, item, index)"
      :class="item.class"
      :target="item.target"
      tabindex="0"
    >
      <i :class="item.icon" class="layout-menuitem-icon"></i>
      <span 
        class="layout-menuitem-text text-lg font-semibold" 
        v-html="highlightedLabel"
      ></span>
      <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items"></i>
    </a>
    <router-link
      v-if="item.to && !item.items && item.visible !== false"
      active-class="text-green-800 font-semibold"
      @click="itemClick($event, item, index)"
      :class="[item.class, { 'active-route': checkActiveRoute(item) }]"
      tabindex="0"
      :to="item.to"
    >
      <i :class="item.icon" class="layout-menuitem-icon"></i>
      <span 
        class="layout-menuitem-text text-lg font-semibold" 
        v-html="highlightedLabel"
      ></span>
      <i class="pi pi-fw pi-angle-down layout-submenu-toggler" v-if="item.items"></i>
    </router-link>
    <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
      <ul v-show="root ? true : isActiveMenu" class="layout-submenu">
        <app-menu-item
          v-for="(child, i) in item.items"
          :key="child"
          :index="i"
          :item="child"
          :parentItemKey="itemKey"
          :root="false"
          :search-query="searchQuery"
        ></app-menu-item>
      </ul>
    </Transition>
  </li>
</template>

<style scoped>
:deep(.search-highlight) {
  background-color: #fef3c7;
  color: #92400e;
  padding: 0.125rem 0.25rem;
  border-radius: 0.25rem;
  font-weight: 600;
}

.search-match {
  animation: highlightPulse 0.5s ease-in-out;
}

@keyframes highlightPulse {
  0%, 100% {
    background-color: transparent;
  }
  50% {
    background-color: rgba(16, 185, 129, 0.1);
  }
}
 
.layout-submenu {
  padding-left: 0;
}
 
.layout-submenu .layout-submenu > li > a,
.layout-submenu .layout-submenu > li > router-link {
  padding-left: 2rem;
} 

.layout-submenu .layout-submenu .layout-submenu > li > a,
.layout-submenu .layout-submenu .layout-submenu > li > router-link {
  padding-left: 3rem;
}

.layout-submenu .layout-submenu .layout-submenu .layout-submenu > li > a,
.layout-submenu .layout-submenu .layout-submenu .layout-submenu > li > router-link {
  padding-left: 2rem;
}
</style>