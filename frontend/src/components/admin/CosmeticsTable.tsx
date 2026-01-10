'use client';

import { useState, useEffect, useCallback } from 'react';
import {
  getAdminCosmetics,
  createAdminCosmetic,
  updateAdminCosmetic,
  deleteAdminCosmetic,
} from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type {
  AdminCosmetic,
  CreateCosmeticRequest,
  UpdateCosmeticRequest,
  CosmeticType,
  CosmeticRarity,
  PaginatedResponse,
} from '@/types';

export interface CosmeticsTableProps {
  refreshTrigger?: number;
}

const COSMETIC_TYPES: CosmeticType[] = ['BoardTheme', 'AvatarFrame', 'Badge'];
const COSMETIC_RARITIES: CosmeticRarity[] = ['Common', 'Rare', 'Epic', 'Legendary'];

const RARITY_COLORS: Record<CosmeticRarity, { bg: string; text: string }> = {
  Common: { bg: 'rgba(148, 163, 184, 0.2)', text: '#94a3b8' },
  Rare: { bg: 'rgba(14, 165, 233, 0.2)', text: '#0ea5e9' },
  Epic: { bg: 'rgba(168, 85, 247, 0.2)', text: '#a855f7' },
  Legendary: { bg: 'rgba(245, 158, 11, 0.2)', text: '#f59e0b' },
};

interface EditValues {
  code: string;
  name: string;
  description: string;
  type: CosmeticType;
  assetUrl: string;
  previewUrl: string;
  rarity: CosmeticRarity;
  isDefault: boolean;
}

const defaultEditValues: EditValues = {
  code: '',
  name: '',
  description: '',
  type: 'BoardTheme',
  assetUrl: '',
  previewUrl: '',
  rarity: 'Common',
  isDefault: false,
};

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
            <div className="h-4 w-24 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
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
            <div className="h-4 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-20 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
        </tr>
      ))}
    </>
  );
}

/**
 * CosmeticsTable - Paginated table for managing cosmetic items.
 * 
 * Features:
 * - Inline editing for all fields
 * - Create new cosmetics
 * - Delete with confirmation and 409 handling
 * - Rarity color coding
 */
export function CosmeticsTable({ refreshTrigger }: CosmeticsTableProps) {
  const token = useToken();
  const [cosmetics, setCosmetics] = useState<PaginatedResponse<AdminCosmetic> | null>(null);
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

  const pageSize = 10;

  const fetchCosmetics = useCallback(async () => {
    if (!token) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await getAdminCosmetics(token, page, pageSize);
      setCosmetics(response);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch cosmetics';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [token, page, pageSize]);

  useEffect(() => {
    fetchCosmetics();
  }, [fetchCosmetics, refreshTrigger]);

  const startEditing = (item: AdminCosmetic) => {
    setEditingRow(item.id);
    setEditValues({
      code: item.code,
      name: item.name,
      description: item.description,
      type: item.type,
      assetUrl: item.assetUrl,
      previewUrl: item.previewUrl,
      rarity: item.rarity,
      isDefault: item.isDefault,
    });
  };

  const cancelEditing = () => {
    setEditingRow(null);
    setEditValues(defaultEditValues);
  };

  const handleSave = async (id: number) => {
    if (!token) return;

    setIsSaving(true);

    try {
      const request: UpdateCosmeticRequest = {
        code: editValues.code,
        name: editValues.name,
        description: editValues.description,
        type: editValues.type,
        assetUrl: editValues.assetUrl,
        previewUrl: editValues.previewUrl,
        rarity: editValues.rarity,
        isDefault: editValues.isDefault,
      };

      await updateAdminCosmetic(id, request, token);
      setEditingRow(null);
      fetchCosmetics();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save';
      setError(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCreate = async () => {
    if (!token) return;

    setIsSaving(true);

    try {
      const request: CreateCosmeticRequest = {
        code: newValues.code,
        name: newValues.name,
        description: newValues.description,
        type: newValues.type,
        assetUrl: newValues.assetUrl,
        previewUrl: newValues.previewUrl,
        rarity: newValues.rarity,
        isDefault: newValues.isDefault,
      };

      await createAdminCosmetic(request, token);
      setIsCreating(false);
      setNewValues(defaultEditValues);
      fetchCosmetics();
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
      await deleteAdminCosmetic(id, token);
      setDeleteConfirmId(null);
      fetchCosmetics();
    } catch (err) {
      if (err instanceof AdminApiError && err.statusCode === 409) {
        setDeleteError('This cosmetic is in use and cannot be deleted.');
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
    if (cosmetics && page < cosmetics.totalPages) setPage(page + 1);
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-white">Cosmetics</h3>
        {!isCreating && (
          <button
            onClick={() => setIsCreating(true)}
            className="px-4 py-2 rounded-lg text-white text-sm font-medium transition-colors"
            style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
          >
            Add Cosmetic
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

      {/* Table */}
      <div
        className="overflow-x-auto rounded-xl backdrop-blur-sm"
        style={{ background: 'rgba(13, 36, 61, 0.85)', border: '1px solid rgba(56, 97, 140, 0.4)' }}
      >
        <table className="w-full min-w-[1100px]">
          <thead>
            <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">ID</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Code</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Name</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Description</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Type</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">URLs</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Rarity</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Default</th>
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
                    type="text"
                    value={newValues.code}
                    onChange={(e) => setNewValues({ ...newValues, code: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    placeholder="unique-code"
                    autoFocus
                  />
                </td>
                <td className="py-3 px-4">
                  <input
                    type="text"
                    value={newValues.name}
                    onChange={(e) => setNewValues({ ...newValues, name: e.target.value })}
                    className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    placeholder="Name"
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
                    value={newValues.type}
                    onChange={(e) => setNewValues({ ...newValues, type: e.target.value as CosmeticType })}
                    className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                  >
                    {COSMETIC_TYPES.map((type) => (
                      <option key={type} value={type} className="bg-gray-800">{type}</option>
                    ))}
                  </select>
                </td>
                <td className="py-3 px-4">
                  <div className="flex flex-col gap-1">
                    <input
                      type="text"
                      value={newValues.assetUrl}
                      onChange={(e) => setNewValues({ ...newValues, assetUrl: e.target.value })}
                      className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      placeholder="Asset URL"
                    />
                    <input
                      type="text"
                      value={newValues.previewUrl}
                      onChange={(e) => setNewValues({ ...newValues, previewUrl: e.target.value })}
                      className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      placeholder="Preview URL"
                    />
                  </div>
                </td>
                <td className="py-3 px-4">
                  <select
                    value={newValues.rarity}
                    onChange={(e) => setNewValues({ ...newValues, rarity: e.target.value as CosmeticRarity })}
                    className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                  >
                    {COSMETIC_RARITIES.map((rarity) => (
                      <option key={rarity} value={rarity} className="bg-gray-800">{rarity}</option>
                    ))}
                  </select>
                </td>
                <td className="py-3 px-4">
                  <input
                    type="checkbox"
                    checked={newValues.isDefault}
                    onChange={(e) => setNewValues({ ...newValues, isDefault: e.target.checked })}
                    className="w-4 h-4 rounded bg-white/10 border-white/20"
                  />
                </td>
                <td className="py-3 px-4">
                  <div className="flex gap-2">
                    <button
                      onClick={handleCreate}
                      disabled={isSaving || !newValues.code || !newValues.name}
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
            ) : !cosmetics || cosmetics.items.length === 0 ? (
              <tr>
                <td colSpan={9} className="py-12 text-center text-gray-500">
                  No cosmetics found.
                </td>
              </tr>
            ) : (
              cosmetics.items.map((item, index) => (
                <tr
                  key={item.id}
                  className="transition-colors hover:bg-white/5"
                  style={{
                    borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
                    background: index % 2 === 0 ? 'rgba(10, 37, 64, 0.3)' : 'transparent',
                  }}
                >
                  <td className="py-3 px-4 text-gray-300 text-sm">{item.id}</td>
                  
                  {/* Code */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="text"
                        value={editValues.code}
                        onChange={(e) => setEditValues({ ...editValues, code: e.target.value })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none font-mono"
                        autoFocus
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-teal-400 text-sm font-mono hover:bg-white/5 px-1 rounded"
                      >
                        {item.code}
                      </span>
                    )}
                  </td>

                  {/* Name */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="text"
                        value={editValues.name}
                        onChange={(e) => setEditValues({ ...editValues, name: e.target.value })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
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
                        className="cursor-pointer text-gray-300 text-sm hover:bg-white/5 px-1 rounded max-w-[150px] truncate block"
                        title={item.description}
                      >
                        {item.description}
                      </span>
                    )}
                  </td>

                  {/* Type */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <select
                        value={editValues.type}
                        onChange={(e) => setEditValues({ ...editValues, type: e.target.value as CosmeticType })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      >
                        {COSMETIC_TYPES.map((type) => (
                          <option key={type} value={type} className="bg-gray-800">{type}</option>
                        ))}
                      </select>
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-xs px-2 py-1 rounded-full font-medium hover:bg-white/5"
                        style={{ background: 'rgba(20, 184, 166, 0.2)', color: '#14b8a6' }}
                      >
                        {item.type}
                      </span>
                    )}
                  </td>

                  {/* URLs */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <div className="flex flex-col gap-1">
                        <input
                          type="text"
                          value={editValues.assetUrl}
                          onChange={(e) => setEditValues({ ...editValues, assetUrl: e.target.value })}
                          className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                          placeholder="Asset URL"
                        />
                        <input
                          type="text"
                          value={editValues.previewUrl}
                          onChange={(e) => setEditValues({ ...editValues, previewUrl: e.target.value })}
                          className="w-full px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                          placeholder="Preview URL"
                        />
                      </div>
                    ) : (
                      <div
                        onClick={() => startEditing(item)}
                        className="cursor-pointer hover:bg-white/5 px-1 rounded"
                      >
                        <div className="text-gray-400 text-xs truncate max-w-[120px]" title={item.assetUrl}>
                          Asset: {item.assetUrl || '-'}
                        </div>
                        <div className="text-gray-500 text-xs truncate max-w-[120px]" title={item.previewUrl}>
                          Preview: {item.previewUrl || '-'}
                        </div>
                      </div>
                    )}
                  </td>

                  {/* Rarity */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <select
                        value={editValues.rarity}
                        onChange={(e) => setEditValues({ ...editValues, rarity: e.target.value as CosmeticRarity })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      >
                        {COSMETIC_RARITIES.map((rarity) => (
                          <option key={rarity} value={rarity} className="bg-gray-800">{rarity}</option>
                        ))}
                      </select>
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-xs px-2 py-1 rounded-full font-medium hover:brightness-110"
                        style={{
                          background: RARITY_COLORS[item.rarity].bg,
                          color: RARITY_COLORS[item.rarity].text,
                        }}
                      >
                        {item.rarity}
                      </span>
                    )}
                  </td>

                  {/* Default */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="checkbox"
                        checked={editValues.isDefault}
                        onChange={(e) => setEditValues({ ...editValues, isDefault: e.target.checked })}
                        className="w-4 h-4 rounded bg-white/10 border-white/20"
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer hover:bg-white/5 px-1 rounded"
                      >
                        {item.isDefault ? (
                          <span className="text-xs px-2 py-0.5 rounded-full bg-teal-500/20 text-teal-400">Default</span>
                        ) : (
                          <span className="text-gray-500">-</span>
                        )}
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
      {cosmetics && cosmetics.totalPages > 0 && (
        <div className="flex justify-between items-center">
          <button
            onClick={handlePrevPage}
            disabled={page <= 1}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Previous
          </button>
          <span className="text-gray-400">
            Page {page} of {cosmetics.totalPages}
          </span>
          <button
            onClick={handleNextPage}
            disabled={page >= cosmetics.totalPages}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
