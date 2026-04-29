<template>
  <div class="menu-search">
    <span class="p-input-icon-left w-full">
      <i class="pi pi-search"></i>
      <InputText
        ref="searchInput"
        v-model="searchQuery"
        @input="handleInput"
        @focus="showDropdown = true"
        placeholder="Tìm kiếm chức năng..."
        class="w-full pl-5"
      />
      <i 
        v-if="isSearching" 
        class="pi pi-times search-clear-icon"
        @click="clearSearch"
      ></i>
    </span>
    
    <transition name="search-results">
      <div v-if="searchResults.length > 0 && showDropdown" class="search-results">
        <div class="search-results-header">
          <span class="text-sm text-muted">
            Tìm thấy {{ searchResults.length }} kết quả
          </span>
        </div>
        <ul class="search-results-list">
          <li 
            v-for="(result, index) in searchResults" 
            :key="index"
            class="search-result-item"
            @click="selectItem(result)"
          >
            <i :class="result.icon" class="result-icon"></i>
            <div class="result-content">
              <div class="result-label">{{ result.label }}</div>
              <div class="result-path">{{ result.pathString }}</div>
            </div>
          </li>
        </ul>
      </div>
      <div v-else-if="isSearching && showDropdown" class="search-results">
        <div class="no-results">
          <i class="pi pi-info-circle"></i>
          <span>Không tìm thấy kết quả phù hợp</span>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import InputText from 'primevue/inputtext';

const props = defineProps({
  menuItems: {
    type: Array,
    default: () => []
  }
});

const emit = defineEmits(['search', 'select']);

const searchQuery = ref('');
const showDropdown = ref(false);
const searchInput = ref(null);
const isSearching = computed(() => searchQuery.value.length > 0);

// Hàm đệ quy để tìm kiếm trong menu với fuzzy search
const searchMenuItems = (items, query, parentPath = []) => {
  let results = [];
  
  items.forEach(item => {
    if (item.visible === false) return;
    
    const currentPath = [...parentPath, item.label];
    
    // Tìm kiếm gần đúng: kiểm tra từng từ trong query có trong label không
    const matchesSearch = fuzzyMatch(item.label, query);
    
    if (matchesSearch && (item.to || item.url)) {
      results.push({
        ...item,
        path: currentPath,
        pathString: currentPath.join(' > ')
      });
    }
    
    if (item.items && item.items.length > 0) {
      const childResults = searchMenuItems(item.items, query, currentPath);
      results = [...results, ...childResults];
    }
  });
  
  return results;
};

// Hàm fuzzy match - tìm kiếm gần đúng
const fuzzyMatch = (text, query) => {
  if (!text || !query) return false;
  
  const textLower = text.toLowerCase().replace(/\s+/g, ' ').trim();
  const queryLower = query.toLowerCase().replace(/\s+/g, ' ').trim();
  
  // Tách query thành các từ
  const queryWords = queryLower.split(' ').filter(word => word.length > 0);
  
  // Kiểm tra tất cả các từ trong query có xuất hiện trong text không
  return queryWords.every(word => textLower.includes(word));
};

const searchResults = computed(() => {
  if (!searchQuery.value.trim()) return [];
  return searchMenuItems(props.menuItems, searchQuery.value).slice(0, 10);
});

const clearSearch = () => {
  searchQuery.value = '';
  showDropdown.value = false;
  emit('search', '');
};

const selectItem = (item) => {
  showDropdown.value = false;
  emit('select', item);
  if (searchInput.value) {
    searchInput.value.$el.blur();
  }
};

const handleInput = () => {
  showDropdown.value = true;
  emit('search', searchQuery.value);
};

// Click outside để đóng dropdown
const handleClickOutside = (event) => {
  const searchElement = event.target.closest('.menu-search');
  if (!searchElement) {
    showDropdown.value = false;
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>


<style scoped>
    @import url('./composables/styleSearch.css');
</style>