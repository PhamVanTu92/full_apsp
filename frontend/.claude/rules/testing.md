# Testing Guidelines

## Hiện trạng
Project hiện có rất ít test. Mục tiêu tăng dần coverage theo thứ tự ưu tiên:
1. Utility functions (`src/helpers/`, `src/utils/`)
2. Pinia stores (business logic)
3. Composables (`src/composables/`)
4. Components quan trọng (form validation, auth flow)

## Setup (cần cài thêm)
```bash
npm install -D vitest @vue/test-utils jsdom @vitest/coverage-v8
```

Thêm vào `vite.config.js`:
```js
test: {
  environment: 'jsdom',
  globals: true,
  coverage: { reporter: ['text', 'html'] }
}
```

## Cách viết test

### Utility functions
```ts
// src/helpers/format.helper.test.ts
import { describe, it, expect } from 'vitest'
import { formatCurrency } from './format.helper'

describe('formatCurrency', () => {
  it('formats VND correctly', () => {
    expect(formatCurrency(1000000)).toBe('1.000.000 ₫')
  })
})
```

### Pinia store
```ts
import { setActivePinia, createPinia } from 'pinia'
import { useExampleStore } from '@/Pinia/example'

beforeEach(() => setActivePinia(createPinia()))

it('fetchItems populates items', async () => {
  const store = useExampleStore()
  await store.fetchItems()
  expect(store.items.length).toBeGreaterThan(0)
})
```

## Quy tắc
- Test file đặt cạnh file nguồn: `format.helper.ts` → `format.helper.test.ts`
- Không mock API calls trong unit test store — dùng mock fetch/axios
- Không test implementation details — test behavior
