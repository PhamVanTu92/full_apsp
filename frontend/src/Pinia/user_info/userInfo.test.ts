import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';

// Mock API module trước khi import store
vi.mock('@/api/api-main', () => ({
    default: {
        get: vi.fn()
    }
}));

import API from '@/api/api-main';
import { useUserInfoStore } from './userInfo';
import type { UserInfo } from './userInfo.interface';

const mockUser: UserInfo = {
    id: 1,
    userName: 'antu',
    fullName: 'An Tú',
    email: 'antu@example.com',
    normalizedEmail: 'ANTU@EXAMPLE.COM',
    normalizedUserName: 'ANTU',
    phone: '0901234567',
    phoneNumber: '0901234567',
    userType: 'staff',
    note: '',
    dob: '1995-01-01',
    emailConfirmed: true,
    status: 'active',
    cardId: null,
    isSupperUser: false,
    roleId: 1,
    personRoleId: 1,
    personRole: {
        id: 1,
        name: 'Admin',
        normalizedName: 'ADMIN',
        concurrencyStamp: null,
        notes: null,
        isActive: true,
        isPersonRole: false,
        countUserInRole: 5,
        privilegeIds: [],
        roleClaims: [],
        partialChecked: false
    },
    claims: [],
    userRoles: null,
    bpInfo: null,
    userGroup: null,
    lastLogin: '2024-01-01T00:00:00Z',
    organizationId: 1,
    directStaff: []
};

describe('useUserInfoStore', () => {
    beforeEach(() => {
        setActivePinia(createPinia());
        vi.clearAllMocks();
    });

    it('initializes userInfo as undefined before fetch', async () => {
        vi.mocked(API.get).mockResolvedValue({ data: mockUser });
        const store = useUserInfoStore();
        // userInfo bắt đầu là undefined trước khi promise resolve
        expect(store.userInfo).toBeUndefined();
    });

    it('populates userInfo after initUserInfoStore resolves', async () => {
        vi.mocked(API.get).mockResolvedValue({ data: mockUser });
        const store = useUserInfoStore();
        await store.initUserInfoStore();
        expect(store.userInfo).toEqual(mockUser);
        expect(store.userInfo?.fullName).toBe('An Tú');
    });

    it('calls API.get with correct endpoint', async () => {
        vi.mocked(API.get).mockResolvedValue({ data: mockUser });
        const store = useUserInfoStore();
        await store.initUserInfoStore();
        expect(API.get).toHaveBeenCalledWith('account/me');
    });

    it('does not call API twice when initUserInfoStore called again (promise cached)', async () => {
        vi.mocked(API.get).mockResolvedValue({ data: mockUser });
        const store = useUserInfoStore();
        await store.initUserInfoStore();
        await store.initUserInfoStore();
        // Lần 2 dùng cached promise — API.get chỉ được gọi 1 lần từ store constructor
        expect(API.get).toHaveBeenCalledTimes(1);
    });

    it('throws and propagates error when API fails', async () => {
        const error = new Error('Network error');
        vi.mocked(API.get).mockRejectedValue(error);
        const store = useUserInfoStore();
        await expect(store.initUserInfoStore()).rejects.toThrow('Network error');
    });
});
