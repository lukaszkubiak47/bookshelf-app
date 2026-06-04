import type { Ref } from 'vue'

export function useWizardStepTransition(currentStep: Ref<number>): {
  goToStep: (step: number) => void
  wizardTransition: Ref<string>
}
