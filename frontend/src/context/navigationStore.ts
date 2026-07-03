import { signal, computed } from "@preact/signals-react";
import type { ModuleModel } from "@/models";

// State signals
const _selectedModule = signal<ModuleModel | null>(null);
const _showBackButton = signal<boolean>(false);

// Computed signals
const selectedModule = computed(() => _selectedModule.value);
const showBackButton = computed(() => _showBackButton.value);
const hasSelectedModule = computed(() => _selectedModule.value !== null);

// Actions
function setSelectedModule(module: ModuleModel | null) {
  _selectedModule.value = module;
  _showBackButton.value = module !== null;
}

function clearSelectedModule() {
  _selectedModule.value = null;
  _showBackButton.value = false;
}

function goBack() {
  clearSelectedModule();
}

export const navigationStore = {
  // State
  selectedModule,
  showBackButton,
  hasSelectedModule,
  
  // Actions
  setSelectedModule,
  clearSelectedModule,
  goBack,
};
