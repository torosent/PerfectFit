'use client';

import { useState, useEffect, useCallback } from 'react';
import {
  getAdminChallengeTemplates,
  createAdminChallengeTemplate,
  updateAdminChallengeTemplate,
  deleteAdminChallengeTemplate,
  activateChallengeTemplate,
  triggerChallengeRotation,
  type ChallengeRotationResponse,
} from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type {
  AdminChallengeTemplate,
  CreateChallengeTemplateRequest,
  UpdateChallengeTemplateRequest,
  ChallengeType,
  PaginatedResponse,
} from '@/types';

export interface ChallengeTemplatesTableProps {
  refreshTrigger?: number;
}

const CHALLENGE_TYPES: ChallengeType[] = ['Daily', 'Weekly'];

interface EditValues {
  name: string;
  description: string;
  type: ChallengeType;
  targetValue: number;
  xpReward: number;
  isActive: boolean;
}

const defaultEditValues: EditValues = {
  name: '',
  description: '',
  type: 'Daily',
  targetValue: 1,
  xpReward: 10,
  isActive: true,
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
            <div className="h-4 w-32 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-48 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-20 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
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
 * ChallengeTemplatesTable - Paginated table for managing challenge templates.
 * 
 * Features:
 * - Inline editing for all fields
 * - Create new templates
 * - Delete with confirmation and 409 handling
 * - Active/inactive toggle
 */
export function ChallengeTemplatesTable({ refreshTrigger }: ChallengeTemplatesTableProps) {
  const token = useToken();
  const [templates, setTemplates] = useState<PaginatedResponse<AdminChallengeTemplate> | null>(null);
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
  const [isRotating, setIsRotating] = useState(false);
  const [rotationResult, setRotationResult] = useState<ChallengeRotationResponse | null>(null);
  const [activatingId, setActivatingId] = useState<number | null>(null);

  const pageSize = 10;

  const fetchTemplates = useCallback(async () => {
    if (!token) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await getAdminChallengeTemplates(token, page, pageSize);
      setTemplates(response);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch templates';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [token, page, pageSize]);

  useEffect(() => {
    fetchTemplates();
  }, [fetchTemplates, refreshTrigger]);

  const startEditing = (item: AdminChallengeTemplate) => {
    setEditingRow(item.id);
    setEditValues({
      name: item.name,
      description: item.description,
      type: item.type,
      targetValue: item.targetValue,
      xpReward: item.xpReward,
      isActive: item.isActive,
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
      const request: UpdateChallengeTemplateRequest = {
        name: editValues.name,
        description: editValues.description,
        type: editValues.type,
        targetValue: editValues.targetValue,
        xpReward: editValues.xpReward,
        isActive: editValues.isActive,
      };

      await updateAdminChallengeTemplate(id, request, token);
      setEditingRow(null);
      fetchTemplates();
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
      const request: CreateChallengeTemplateRequest = {
        name: newValues.name,
        description: newValues.description,
        type: newValues.type,
        targetValue: newValues.targetValue,
        xpReward: newValues.xpReward,
      };

      await createAdminChallengeTemplate(request, token);
      setIsCreating(false);
      setNewValues(defaultEditValues);
      fetchTemplates();
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
      await deleteAdminChallengeTemplate(id, token);
      setDeleteConfirmId(null);
      fetchTemplates();
    } catch (err) {
      if (err instanceof AdminApiError && err.statusCode === 409) {
        setDeleteError('This template has active challenges and cannot be deleted.');
      } else {
        const message = err instanceof Error ? err.message : 'Failed to delete';
        setDeleteError(message);
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleToggleActive = async (item: AdminChallengeTemplate) => {
    if (!token) return;

    setIsSaving(true);

    try {
      const request: UpdateChallengeTemplateRequest = {
        name: item.name,
        description: item.description,
        type: item.type,
        targetValue: item.targetValue,
        xpReward: item.xpReward,
        isActive: !item.isActive,
      };

      await updateAdminChallengeTemplate(item.id, request, token);
      fetchTemplates();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to update';
      setError(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleRotateAll = async () => {
    if (!token) return;

    setIsRotating(true);
    setRotationResult(null);
    setError(null);

    try {
      const result = await triggerChallengeRotation(token);
      setRotationResult(result);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to rotate challenges';
      setError(message);
    } finally {
      setIsRotating(false);
    }
  };

  const handleActivateSingle = async (templateId: number) => {
    if (!token) return;

    setActivatingId(templateId);
    setError(null);

    try {
      const result = await activateChallengeTemplate(templateId, token);
      setRotationResult({
        createdChallenges: [{ challengeId: result.challengeId, name: result.name, type: result.type }],
        skippedTemplates: [],
        message: result.message,
      });
    } catch (err) {
      if (err instanceof AdminApiError && err.statusCode === 409) {
        setError('A challenge from this template is already active.');
      } else {
        const message = err instanceof Error ? err.message : 'Failed to activate';
        setError(message);
      }
    } finally {
      setActivatingId(null);
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
    if (templates && page < templates.totalPages) setPage(page + 1);
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex justify-between items-center flex-wrap gap-2">
        <h3 className="text-lg font-semibold text-white">Challenge Templates</h3>
        <div className="flex gap-2">
          <button
            onClick={handleRotateAll}
            disabled={isRotating}
            className="px-4 py-2 rounded-lg text-white text-sm font-medium transition-colors disabled:opacity-50 bg-purple-600 hover:bg-purple-700"
            title="Create active challenges from all active templates"
          >
            {isRotating ? '🔄 Rotating...' : '🔄 Rotate All'}
          </button>
          {!isCreating && (
            <button
              onClick={() => setIsCreating(true)}
              className="px-4 py-2 rounded-lg text-white text-sm font-medium transition-colors"
              style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
            >
              Add Template
            </button>
          )}
        </div>
      </div>

      {/* Rotation Result */}
      {rotationResult && (
        <div className="p-4 rounded-lg bg-green-500/10 border border-green-500/30 text-green-400">
          <div className="font-medium mb-2">✅ {rotationResult.message}</div>
          {rotationResult.createdChallenges.length > 0 && (
            <div className="text-sm">
              <span className="font-medium">Created:</span>{' '}
              {rotationResult.createdChallenges.map((c) => `${c.name} (${c.type})`).join(', ')}
            </div>
          )}
          {rotationResult.skippedTemplates.length > 0 && (
            <div className="text-sm text-yellow-400">
              <span className="font-medium">Skipped:</span>{' '}
              {rotationResult.skippedTemplates.join(', ')}
            </div>
          )}
          <button onClick={() => setRotationResult(null)} className="mt-2 text-sm underline">Dismiss</button>
        </div>
      )}

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
        <table className="w-full min-w-[900px]">
          <thead>
            <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">ID</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Name</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Description</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Type</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Target</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">XP Reward</th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">Active</th>
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
                    value={newValues.type}
                    onChange={(e) => setNewValues({ ...newValues, type: e.target.value as ChallengeType })}
                    className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                  >
                    {CHALLENGE_TYPES.map((type) => (
                      <option key={type} value={type} className="bg-gray-800">{type}</option>
                    ))}
                  </select>
                </td>
                <td className="py-3 px-4">
                  <input
                    type="number"
                    value={newValues.targetValue}
                    onChange={(e) => setNewValues({ ...newValues, targetValue: parseInt(e.target.value) || 0 })}
                    className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    min={1}
                  />
                </td>
                <td className="py-3 px-4">
                  <input
                    type="number"
                    value={newValues.xpReward}
                    onChange={(e) => setNewValues({ ...newValues, xpReward: parseInt(e.target.value) || 0 })}
                    className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                    min={0}
                  />
                </td>
                <td className="py-3 px-4">
                  <span className="text-gray-500 text-sm">Auto</span>
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
            ) : !templates || templates.items.length === 0 ? (
              <tr>
                <td colSpan={8} className="py-12 text-center text-gray-500">
                  No challenge templates found.
                </td>
              </tr>
            ) : (
              templates.items.map((item, index) => (
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

                  {/* Type */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <select
                        value={editValues.type}
                        onChange={(e) => setEditValues({ ...editValues, type: e.target.value as ChallengeType })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                      >
                        {CHALLENGE_TYPES.map((type) => (
                          <option key={type} value={type} className="bg-gray-800">{type}</option>
                        ))}
                      </select>
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-xs px-2 py-1 rounded-full font-medium hover:bg-white/5"
                        style={{
                          background: item.type === 'Daily' ? 'rgba(14, 165, 233, 0.2)' : 'rgba(168, 85, 247, 0.2)',
                          color: item.type === 'Daily' ? '#0ea5e9' : '#a855f7',
                        }}
                      >
                        {item.type}
                      </span>
                    )}
                  </td>

                  {/* Target Value */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="number"
                        value={editValues.targetValue}
                        onChange={(e) => setEditValues({ ...editValues, targetValue: parseInt(e.target.value) || 0 })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        min={1}
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-gray-300 text-sm hover:bg-white/5 px-1 rounded"
                      >
                        {item.targetValue}
                      </span>
                    )}
                  </td>

                  {/* XP Reward */}
                  <td className="py-3 px-4">
                    {editingRow === item.id ? (
                      <input
                        type="number"
                        value={editValues.xpReward}
                        onChange={(e) => setEditValues({ ...editValues, xpReward: parseInt(e.target.value) || 0 })}
                        onKeyDown={(e) => handleKeyDown(e, item.id)}
                        className="w-20 px-2 py-1 rounded bg-white/10 text-white text-sm border border-white/20 focus:border-teal-400 focus:outline-none"
                        min={0}
                      />
                    ) : (
                      <span
                        onClick={() => startEditing(item)}
                        className="cursor-pointer text-teal-400 text-sm font-medium hover:bg-white/5 px-1 rounded"
                      >
                        {item.xpReward} XP
                      </span>
                    )}
                  </td>

                  {/* Active Toggle */}
                  <td className="py-3 px-4">
                    <button
                      onClick={() => handleToggleActive(item)}
                      disabled={isSaving}
                      className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                        item.isActive ? 'bg-teal-500' : 'bg-gray-600'
                      } disabled:opacity-50`}
                      aria-label={item.isActive ? 'Deactivate' : 'Activate'}
                    >
                      <span
                        className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                          item.isActive ? 'translate-x-6' : 'translate-x-1'
                        }`}
                      />
                    </button>
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
                      <div className="flex gap-2">
                        {item.isActive && (
                          <button
                            onClick={() => handleActivateSingle(item.id)}
                            disabled={activatingId === item.id}
                            className="px-3 py-1.5 rounded text-sm text-purple-400 hover:bg-purple-500/10 transition-colors disabled:opacity-50"
                            title="Create an active challenge from this template"
                          >
                            {activatingId === item.id ? '...' : '▶️ Activate'}
                          </button>
                        )}
                        <button
                          onClick={() => setDeleteConfirmId(item.id)}
                          className="px-3 py-1.5 rounded text-sm text-red-400 hover:bg-red-500/10 transition-colors"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {templates && templates.totalPages > 0 && (
        <div className="flex justify-between items-center">
          <button
            onClick={handlePrevPage}
            disabled={page <= 1}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Previous
          </button>
          <span className="text-gray-400">
            Page {page} of {templates.totalPages}
          </span>
          <button
            onClick={handleNextPage}
            disabled={page >= templates.totalPages}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
