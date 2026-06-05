/**
 * Composable: animacja przejścia między krokami kreatora „Dodaj książkę”.
 *
 * Cel: ustawia kierunek slajdu (do przodu / do tyłu) na podstawie numeru kroku.
 * Kiedy używać: w widoku wieloetapowego formularza, gdzie <Transition :name="wizardTransition">.
 *
 * Powiązane pliki:
 * - views/AddBookView.vue — główny konsument (currentStep + goToStep)
 * - assets/main.css — klasy animacji wizard-forward / wizard-back
 */
import { ref, computed } from 'vue';

export function useWizardStepTransition(currentStep) {
  const direction = ref('forward');

  // --- Nawigacja między krokami — kierunek zależy od porównania numerów ---

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
