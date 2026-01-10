'use client';

import { useState, useCallback } from 'react';
import Link from 'next/link';
import { motion } from 'motion/react';
import { UsersTable } from '@/components/admin/UsersTable';
import { AuditLogTable } from '@/components/admin/AuditLogTable';
import { GamificationTab } from '@/components/admin/GamificationTab';
import { DeleteUserModal } from '@/components/admin/DeleteUserModal';
import { BulkDeleteModal } from '@/components/admin/BulkDeleteModal';
import type { AdminUser } from '@/types';

type Tab = 'users' | 'audit-logs' | 'gamification';

/**
 * Admin Portal Dashboard
 * 
 * Features:
 * - Tab navigation: Users | Audit Logs
 * - UsersTable with delete functionality
 * - AuditLogTable for viewing admin actions
 */
export default function AdminPage() {
  const [activeTab, setActiveTab] = useState<Tab>('users');
  const [userToDelete, setUserToDelete] = useState<AdminUser | null>(null);
  const [showBulkDeleteModal, setShowBulkDeleteModal] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleDeleteClick = useCallback((user: AdminUser) => {
    setUserToDelete(user);
  }, []);

  const handleBulkDeleteClick = useCallback(() => {
    setShowBulkDeleteModal(true);
  }, []);

  const handleDeleteSuccess = useCallback(() => {
    setRefreshTrigger((prev) => prev + 1);
  }, []);

  const handleCloseDeleteModal = useCallback(() => {
    setUserToDelete(null);
  }, []);

  const handleCloseBulkDeleteModal = useCallback(() => {
    setShowBulkDeleteModal(false);
  }, []);

  return (
    <div className="min-h-screen" style={{ backgroundColor: '#0a1628' }}>
      {/* Header */}
      <header
        className="sticky top-0 z-40 backdrop-blur-sm"
        style={{ 
          background: 'rgba(10, 22, 40, 0.95)',
          borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
        }}
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-white">
              Admin Portal
            </h1>
            <Link
              href="/"
              className="text-gray-400 hover:text-white transition-colors text-sm"
            >
              ← Back to Game
            </Link>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Tab navigation */}
        <div className="flex gap-2 mb-8">
          <button
            onClick={() => setActiveTab('users')}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === 'users'
                ? 'text-white'
                : 'text-gray-400 hover:text-white hover:bg-white/5'
            }`}
            style={
              activeTab === 'users'
                ? { background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }
                : undefined
            }
          >
            Users
          </button>
          <button
            onClick={() => setActiveTab('audit-logs')}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === 'audit-logs'
                ? 'text-white'
                : 'text-gray-400 hover:text-white hover:bg-white/5'
            }`}
            style={
              activeTab === 'audit-logs'
                ? { background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }
                : undefined
            }
          >
            Audit Logs
          </button>
          <button
            onClick={() => setActiveTab('gamification')}
            className={`px-4 py-2 rounded-lg font-medium transition-colors ${
              activeTab === 'gamification'
                ? 'text-white'
                : 'text-gray-400 hover:text-white hover:bg-white/5'
            }`}
            style={
              activeTab === 'gamification'
                ? { background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }
                : undefined
            }
          >
            Gamification
          </button>
        </div>

        {/* Tab content */}
        <motion.div
          key={activeTab}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.2 }}
        >
          {activeTab === 'users' && (
            <UsersTable
              onDeleteClick={handleDeleteClick}
              onBulkDeleteClick={handleBulkDeleteClick}
              refreshTrigger={refreshTrigger}
            />
          )}
          {activeTab === 'audit-logs' && (
            <AuditLogTable refreshTrigger={refreshTrigger} />
          )}
          {activeTab === 'gamification' && (
            <GamificationTab refreshTrigger={refreshTrigger} />
          )}
        </motion.div>
      </main>

      {/* Delete User Modal */}
      {userToDelete && (
        <DeleteUserModal
          user={userToDelete}
          isOpen={true}
          onClose={handleCloseDeleteModal}
          onSuccess={handleDeleteSuccess}
        />
      )}

      {/* Bulk Delete Modal */}
      <BulkDeleteModal
        isOpen={showBulkDeleteModal}
        onClose={handleCloseBulkDeleteModal}
        onSuccess={handleDeleteSuccess}
      />
    </div>
  );
}
