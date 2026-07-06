import type { ReactNode } from 'react';
import styles from './Tabs.module.css';

export interface TabItem {
  id: string;
  label: string;
  icon?: ReactNode;
  badge?: number | string;
  disabled?: boolean;
  description?: string;
}

interface TabsProps {
  tabs: TabItem[];
  activeTab: string;
  onChange: (tabId: string) => void;
  variant?: 'default' | 'pill';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  ariaLabel?: string;
  className?: string;
}

export const Tabs = ({
  tabs,
  activeTab,
  onChange,
  variant = 'default',
  size = 'md',
  fullWidth = false,
  ariaLabel,
  className = '',
}: TabsProps) => (
  <div
    className={`${styles.tabsContainer} ${styles[variant]} ${styles[size]} ${fullWidth ? styles.fullWidth : ''} ${className}`}
    role="tablist"
    aria-label={ariaLabel || 'Tabs'}
  >
    {tabs.map((tab) => (
      <button
        key={tab.id}
        id={`tab-${tab.id}`}
        role="tab"
        aria-selected={activeTab === tab.id}
        aria-controls={`tabpanel-${tab.id}`}
        disabled={tab.disabled}
        onClick={() => { if (!tab.disabled) onChange(tab.id); }}
        className={`${styles.tab} ${activeTab === tab.id ? styles.active : styles.inactive} ${tab.disabled ? styles.disabled : ''}`}
        title={tab.description || tab.label}
      >
        {tab.icon && (
          <span className={styles.icon} aria-hidden="true">
            {tab.icon}
          </span>
        )}

        <span className={styles.label}>{tab.label}</span>

        {tab.badge != null && (
          <span className={styles.badge} aria-label={`${tab.badge} items`}>
            {tab.badge}
          </span>
        )}

        {activeTab === tab.id && variant === 'default' && (
          <span className={styles.activeIndicator} aria-hidden="true" />
        )}
      </button>
    ))}
  </div>
);
