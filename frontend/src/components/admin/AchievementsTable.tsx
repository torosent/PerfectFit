'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import {
  getAdminAchievements,
  createAdminAchievement,
  updateAdminAchievement,
  deleteAdminAchievement,
} from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type {
  AdminAchievement,
  CreateAchievementRequest,
  UpdateAchievementRequest,
  AchievementCategory,
  RewardType,
  PaginatedResponse,
} from '@/types';

export interface AchievementsTableProps {
  refreshTrigger?: number;
}

const ACHIEVEMENT_CATEGORIES: AchievementCategory[] = ['Score', 'Streak', 'Games', 'Challenge', 'Special'];
const REWARD_TYPES: RewardType[] = ['Cosmetic', 'StreakFreeze', 'XPBoost'];

interface EditValues {
  name: string;
  description: string;
  category: AchievementCategory;
  iconUrl: string;
  unlockCondition: string;
  rewardType: RewardType;
  rewardValue: number;
  isSecret: boolean;
  displayOrder: number;
  rewardCosmeticCode: string;
}

const defaultEditValues: EditValues = {
  name: '',
  description: '',
  category: 'Score',
  iconUrl: '',
  unlockCondition: '{}',
  rewardType: 'XPBoost',
  rewardValue: 0,
  isSecret: false,
  displayOrder: 0,
  rewardCosmeticCode: '',
};

/**
 * Validate JSON string
 */
function isValidJson(str: string): boolean {
  try {
    JSON.parse(str);
    return true;
  } catch {
    return false;
  }
}

/**
 * Loading skeleton for table rows
 */
function TableSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="border-b border-gray-800">
          <td className="py-3 px-4">
            <div className="h-4 w-8 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-32 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-48 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-20 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-24 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-20 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-24 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
        </tr>
      ))}
    </>
  );
}

/**
 * AchievementsTable - Paginated table for managing achievements.
 * 
 * Features:
 * - Inline editing for all fields
 * - JSON validation for unlockCondition
 * - Create new achievements
 * - Delete with confirmation and 409 handling
 */
export function AchievementsTable({ refreshTrigger }: AchievementsTableProps) {
  const token = useToken();
  const [achievements, setAchievements] = useState<PaginatedResponse<AdminAchievement> | null>(null);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingRow, setEditingRow] = useState<number | null>(null);
  const [editValues, setEditValues] = useState<EditValues>(defaultEditValues);
  const [isSaving, setIsSaving] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [newValues, setNewValues] = useState<EditValues>(defaultEditValues);
  const [deleteConfirmId, setDeleteConfirmId] = useState<number | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [jsonError, setJsonError] = useState<string | null>(null);

  const pageSize = 10;
  const inputRef = useRef<HTMLInputElement>(null);

  const fetchAchievements = useCallback(async () => {
    if (!token) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await getAdminAchievements(token, page, pageSize);
      setAchievements(response);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch achievements';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [token, page, pageSize]);

  useEffect(() => {
    fetchAchievements();
  }, [fetchAchievements, refreshTrigger]);

  const startEditing = (item: AdminAchievement) => {
    setEditingRow(item.id);
    setEditValues({
      name: item.name,
      description: item.description,
      category: item.category,
      iconUrl: item.iconUrl,
      unlockCondition: item.unlockCondition,
      rewardType: item.rewardType,
      rewardValue: item.rewardValue,
      isSecret: item.isSecret,
      displayOrder: item.displayOrder,
      rewardCosmeticCode: item.rewardCosmeticCode || '',
    });
    setJsonError(null);
  };

  const cancelEditing = () => {
    setEditingRow(null);
    setEditValues(defaultEditValues);
    setJsonError(null);
  };

  const handleSave = async (id: number) => {
    if (!token) return;

    // Validate JSON
    if (!isValidJson(editValues.unlockCondition)) {
      setJsonError('Invalid JSON in unlock condition');
      return;
    }

    setIsSaving(true);
    setJsonError(null);

    try {
      const request: UpdateAchievementRequest = {
        name: editValues.name,
        description: editValues.description,
        category: editValues.category,
        iconUrl: editValues.iconUrl,
        unlockCondition: editValues.unlockCondition,
        rewardType: editValues.rewardType,
        rewardValue: editValues.rewardValue,
        isSecret: editValues.isSecret,
        displayOrder: editValues.displayOrder,
        rewardCosmeticCode: editValues.rewardCosmeticCode || null,
      };

      await updateAdminAchievement(id, request, token);
      setEditingRow(null);
      fetchAchievements();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save';
      setError(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCreate = async () => {
    if (!token) return;

    // Validate JSON
    if (!isValidJson(newValues.unlockCondition)) {
      setJsonError('Invalid JSON in unlock condition');
      return;
    }

    setIsSaving(true);
    setJsonError(null);

    try {
      const request: CreateAchievementRequest = {
        name: newValues.name,
        description: newValues.description,
        category: newValues.category,
        iconUrl: newValues.iconUrl,
        unlockCondition: newValues.unlockCondition,
        rewardType: newValues.rewardType,
        rewardValue: newValues.rewardValue,
        isSecret: newValues.isSecret,
        displayOrder: newValues.displayOrder,
        rewardCosmeticCode: newValues.rewardCosmeticCode || null,
      };

      await createAdminAchievement(request, token);
      setIsCreating(false);
      setNewValues(defaultEditValues);
      fetchAchievements();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to create';
      setError(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!token) return;

    setIsSaving(true);
    setDeleteError(null);

    try {
      await deleteAdminAchievement(id, token);
      setDeleteConfirmId(null);
      fetchAchievements();
    } catch (err) {
      if (err instanceof AdminApiError && err.statusCode === 409) {
        setDeleteError('This achievement is in use and cannot be deleted.');
      } else {
        const message = err instanceof Error ? err.message : 'Failed to delete';
        setDeleteError(message);
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent, id: number) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSave(id);
    }
    if (e.key === 'Escape') {
      cancelEditing();
    }
  };

  const handlePrevPage = () => {
    if (page > 1) setPage(page - 1);
  };

  const handleNextPage = () => {
    if (achievements && page < achievements.totalPages) setPage(page + 1);
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-white">Achievements</h3>
        {!isCreating && (
          <button
            onClick={() => setIsCreating(true)}
            className="px-4 py-2 rounded-lg text-white text-sm font-medium transition-colors"
            style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
          >
            Add Achievement
          </button>
        )}
      </div>

      {/* Error state */}
      {error && (
        <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400">
          {error}
          <button onClick={() => setError(null)} className="ml-2 underline">Dismiss</button>
        </div>
      )}

      {/* JSON validation error */}
      {jsonError && (
        <div className="p-4 rounded-lg bg-yellow-500/10 border border-yellow-500/30 text-yellow-400">
          {jsonError}
        </div>
      )}

      {/* Table */}
      <div
        className="overflow-x-auto rounded-xl backdrop-blur-sm"
        style={{ background: 'rgba(13, 36, 61, 0.85)', border: '1px solid rgba(56, 97, 140, 0.4)' }}
      >
        <table className="w-full min-w-[1200px]">
          <thead>
            <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">ID</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Name</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Description</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Category</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Icon URL</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Unlock Condition</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Reward</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Secret</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Order</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody>
            {/* Create new row */}
            {isCreating && (
              <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.3)', background: 'rgba(20, 184, 166, 0.1)' }}>
                <td className="py-3 px-4 text-gray-500 text-sm">New</td>
                <td className="py-3 px-4">
                  <input
                    ref={inputRef}
                    type="text"
                    value={newValues.name}
                    onChange={(e) => setNewValues({ ...newValues, name: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    placeholder="Name"
                    autoFocus
                  />
                </td>
                <td className="py-3 px-4">
                  <input
                    type="text"
                    value={newValues.description}
                    onChange={(e) => setNewValues({ ...newValues, description: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    placeholder="Description"
                  />
                </td>
                <td className="py-3 px-4">
                  <select
                    value={newValues.category}
                    onChange={(e) => setNewValues({ ...newValues, category: e.target.value as AchievementCategory })}
                    className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                  >
                    {ACHIEVEMENT_CATEGORIES.map((cat) => (
                      <option key={cat} value={cat} className="bg-gray-800">{cat}</option>
                    ))}
                  </select>
                </td>
                <td className="py-3 px-4">
                  <input
                    type="text"
                    value={newValues.iconUrl}
                    onChange={(e) => setNewValues({ ...newValues, iconUrl: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    placeholder="Icon URL"
                  />
                </td>
                <td className="py-3 px-4">
                  <textarea
                    value={newValues.unlockCondition}
                    onChange={(e) => setNewValues({ ...newValues, unlockCondition: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none font-mono min-h-[60px]"
                    placeholder="{}"
                  />
                </td>
                <td className="py-3 px-4">
                  <div className="flex flex-col gap-1">
                    <select
                      value={newValues.rewardType}
                      onChange={(e) => setNewValues({ ...newValues, rewardType: e.target.value as RewardType })}
                      className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    >
                      {REWARD_TYPES.map((type) => (
                        <option key={type} value={type} className="bg-gray-800">{type}</option>
                      ))}
                    </select>
                    <input
                      type="number"
                      value={newValues.rewardValue}
                      onChange={(e) => setNewValues({ ...newValues, rewardValue: parseInt(e.target.value) || 0 })}
                      className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      placeholder="Value"
                    />
                    {newValues.rewardType === 'Cosmetic' && (
                      <input
                        type="text"
                        value={newValues.rewardCosmeticCode}
                        onChange={(e) => setNewValues({ ...newValues, rewardCosmeticCode: e.target.value })}
                        className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        placeholder="Cosmetic Code"
                      />
                    )}
                  </div>
                </td>
                <td className="py-3 px-4">
                  <input
                    type="checkbox"
                    checked={newValues.isSecret}
                    onChange={(e) => setNewValues({ ...newValues, isSecret: e.target.checked })}
                    className="w-4 h-4 rounded bg-white/10 border-white/20"
                  />
                </td>
                <td className="py-3 px-4">
                  <input
                    type="number"
                    value={newValues.displayOrder}
                    onChange={(e) => setNewValues({ ...newValues, displayOrder: parseInt(e.target.value) || 0 })}
                    className="w-16 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                  />
                </td>
                <td className="py-3 px-4">
                  <div className="flex gap-2">
                    <button
                      onClick={handleCreate}
                      disabled={isSaving || !newValues.name}
                      className="px-3 py-1.5 rounded text-sm text-white transition-colors disabled:opacity-50"
                      style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
                    >
                      {isSaving ? 'Saving...' : 'Save'}
                    </button>
                    <button
                      onClick={() => { setIsCreating(false); setNewValues(defaultEditValues); }}
                      className="px-3 py-1.5 rounded text-sm text-gray-400 hover:bg-white/10 transition-colors"
                    >
                      Cancel
                    </button>
                  </div>
                </td>
              </tr>
            )}

            {isLoading ? (
              <TableSkeleton />
            ) : !achievements || achievements.items.length === 0 ? (
              <tr>
                <td colSpan={10} className="py-12 text-center text-gray-500">
                  No achievements found.
                </td>
              </tr>
            ) : (
              achievements.items.map((item, index) => (
                <tr
                  key={item.id}
                  className="transition-colors hover:bg-white/5"
                  style={{
                    borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
                    background: index % 2 === 0 ? 'rgba(10, 37, 64, 0.3)' : 'transparent',
                  }}
                >
                  <td className="py-3 px-4 text-gray-300 text-sm">{item.id}</td>
                  
                  {/* Name */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="text"
                        value={editValues.name}
                        onChange={(e) => setEditValues({ ...editValues, name: e.target.value })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        autoFocus
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-white text-sm hover:bg-white/5 px-1 rounded"
                      >
                        {item.name}
                      </span>
                    )}
                  </td>

                  {/* Description */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="text"
                        value={editValues.description}
                        onChange={(e) => setEditValues({ ...editValues, description: e.target.value })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-300 text-sm hover:bg-white/5 px-1 rounded max-w-[200px] truncate block"
                        title={item.description}
                      >
                        {item.description}
                      </span>
                    )}
                  </td>

                  {/* Category */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <select
                        value={editValues.category}
                        onChange={(e) => setEditValues({ ...editValues, category: e.target.value as AchievementCategory })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      >
                        {ACHIEVEMENT_CATEGORIES.map((cat) => (
                          <option key={cat} value={cat} className="bg-gray-800">{cat}</option>
                        ))}
                      </select>
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-xs px-2 py-1 rounded-full font-medium hover:bg-white/5"
                        style={{ background: 'rgba(20, 184, 166, 0.2)', color: '#14b8a6' }}
                      >
                        {item.category}
                      </span>
                    )}
                  </td>

                  {/* Icon URL */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="text"
                        value={editValues.iconUrl}
                        onChange={(e) => setEditValues({ ...editValues, iconUrl: e.target.value })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-400 text-sm hover:bg-white/5 px-1 rounded max-w-[100px] truncate block"
                        title={item.iconUrl}
                      >
                        {item.iconUrl || '-'}
                      </span>
                    )}
                  </td>

                  {/* Unlock Condition (JSON) */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <textarea
                        value={editValues.unlockCondition}
                        onChange={(e) => setEditValues({ ...editValues, unlockCondition: e.target.value })}
                        onKeyDown={(e) => {
                          if (e.key === 'Escape') cancelEditing();
                        }}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none font-mono min-h-[60px]"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-400 text-xs font-mono hover:bg-white/5 px-1 rounded max-w-[150px] truncate block"
                        title={item.unlockCondition}
                      >
                        {item.unlockCondition}
                      </span>
                    )}
                  </td>

                  {/* Reward */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <div className="flex flex-col gap-1">
                        <select
                          value={editValues.rewardType}
                          onChange={(e) => setEditValues({ ...editValues, rewardType: e.target.value as RewardType })}
                          className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        >
                          {REWARD_TYPES.map((type) => (
                            <option key={type} value={type} className="bg-gray-800">{type}</option>
                          ))}
                        </select>
                        <input
                          type="number"
                          value={editValues.rewardValue}
                          onChange={(e) => setEditValues({ ...editValues, rewardValue: parseInt(e.target.value) || 0 })}
                          className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        />
                        {editValues.rewardType === 'Cosmetic' && (
                          <input
                            type="text"
                            value={editValues.rewardCosmeticCode}
                            onChange={(e) => setEditValues({ ...editValues, rewardCosmeticCode: e.target.value })}
                            className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                            placeholder="Cosmetic Code"
                          />
                        )}
                      </div>
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-300 text-sm hover:bg-white/5 px-1 rounded"
                      >
                        {item.rewardType}: {item.rewardValue}
                        {item.rewardCosmeticCode && <span className="text-gray-500 ml-1">({item.rewardCosmeticCode})</span>}
                      </span>
                    )}
                  </td>

                  {/* Secret */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="checkbox"
                        checked={editValues.isSecret}
                        onChange={(e) => setEditValues({ ...editValues, isSecret: e.target.checked })}
                        className="w-4 h-4 rounded bg-white/10 border-white/20"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer hover:bg-white/5 px-1 rounded"
                      >
                        {item.isSecret ? (
                          <span className="text-xs px-2 py-0.5 rounded-full bg-purple-500/20 text-purple-400">Secret</span>
                        ) : (
                          <span className="text-gray-500">-</span>
                        )}
                      </span>
                    )}
                  </td>

                  {/* Display Order */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="number"
                        value={editValues.displayOrder}
                        onChange={(e) => setEditValues({ ...editValues, displayOrder: parseInt(e.target.value) || 0 })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-16 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-400 text-sm hover:bg-white/5 px-1 rounded"
                      >
                        {item.displayOrder}
                      </span>
                    )}
                  </td>

                  {/* Actions */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleSave(item.id)}
                          disabled={isSaving}
                          className="px-3 py-1.5 rounded text-sm text-white transition-colors disabled:opacity-50"
                          style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
                        >
                          {isSaving ? '...' : 'Save'}
                        </button>
                        <button
                          onClick={cancelEditing}
                          className="px-3 py-1.5 rounded text-sm text-gray-400 hover:bg-white/10 transition-colors"
                        >
                          Cancel
                        </button>
                      </div>
                    ) : deleteConfirmId === item.id ? (
                      <div className="flex flex-col gap-2">
                        {deleteError && (
                          <span className="text-xs text-red-400">{deleteError}</span>
                        )}
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleDelete(item.id)}
                            disabled={isSaving}
                            className="px-3 py-1.5 rounded text-sm text-white bg-red-600 hover:bg-red-700 transition-colors disabled:opacity-50"
                          >
                            {isSaving ? '...' : 'Confirm'}
                          </button>
                          <button
                            onClick={() => { setDeleteConfirmId(null); setDeleteError(null); }}
                            className="px-3 py-1.5 rounded text-sm text-gray-400 hover:bg-white/10 transition-colors"
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    ) : (
                      <button
                        onClick={() => setDeleteConfirmId(item.id)}
                        className="px-3 py-1.5 rounded text-sm text-red-400 hover:bg-red-500/10 transition-colors"
                      >
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {achievements && achievements.totalPages > 0 && (
        <div className="flex justify-between items-center">
          <button
            onClick={handlePrevPage}
            disabled={page <= 1}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Previous
          </button>
          <span className="text-gray-400">
            Page {page} of {achievements.totalPages}
          </span>
          <button
            onClick={handleNextPage}
            disabled={page >= achievements.totalPages}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
