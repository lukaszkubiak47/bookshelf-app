/**
 * Kreator „Dodaj książkę”: kierunek animacji przy zmianie kroku (wizard-forward / wizard-back).
 */
import { ref, computed } from 'vue';

/** Przejścia kroków kreatora — slide jak w mockupie HTML. */
export function useWizardStepTransition(currentStep) {
  const direction = ref('forward');

  function goToStep(step) {
    if (step === currentStep.value) return;
    direction.value = step > currentStep.value ? 'forward' : 'back';
    currentStep.value = step;
  }

  const wizardTransition = computed(() =>
    (direction.value === 'forward' ? 'wizard-forward' : 'wizard-back'),
  );

  return { goToStep, wizardTransition };
}
