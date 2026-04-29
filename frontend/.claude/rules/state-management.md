# State Management Rules

## Quy tắc chính: Dùng Pinia, KHÔNG thêm Vuex mới

Project đang trong quá trình migrate từ Vuex → Pinia.
- `src/Pinia/` — **stores mới, đây là nguồn state duy nhất**
- `src/store/` — **Vuex legacy, chỉ auth module, KHÔNG thêm mới**

## Cấu trúc Pinia Store
```ts
// src/Pinia/example.ts
import { defineStore } from 'pinia'

export const useExampleStore = defineStore('example', () => {
  // state
  const items = ref<Item[]>([])
  const loading = ref(false)

  // getters
  const total = computed(() => items.value.length)

  // actions
  async function fetchItems() {
    loading.value = true
    try {
      items.value = await apiMain.get('/items')
    } finally {
      loading.value = false
    }
  }

  return { items, loading, total, fetchItems }
})
```

## Quy tắc đặt tên store
- File: `src/Pinia/<ModuleName>.ts` hoặc `src/Pinia/<Feature>/<name>.ts`
- Store ID: kebab-case (`'purchase-order'`, `'user-info'`)
- Composable: `use<Name>Store()` (`usePurchaseOrderStore()`)

## Filter Stores
Các filter store nằm trong `src/Pinia/Filter/`:
- Dùng cho filter state của từng màn hình danh sách
- Persist filter state khi navigate và quay lại

## Khi nào KHÔNG dùng store
- State chỉ cần trong 1 component → `ref()` / `reactive()` local
- State truyền giữa parent-child → `props` + `emits`
- State chia sẻ giữa sibling → lift lên parent hoặc dùng store
