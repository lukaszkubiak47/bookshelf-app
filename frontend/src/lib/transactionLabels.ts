/**
 * Polskie opisy typów zdarzeń transakcji (pole Type z kolejki transaction-events).
 *
 * Kiedy tu zajrzeć: oś czasu w karcie (BookOffer.vue) i lista powiadomień przy dzwonku (AppNavbar.vue).
 *
 * Eksporty: historyLabels (oś czasu), notificationLabels (z perspektywy odbiorcy), labelFor.
 * Backend: typy z Bookshelf.API/Models/Events/TransactionEvent.cs.
 */

/** Oś czasu transakcji: co się stało z prośbą */
export const historyLabels: Record<string, string> = {
  Requested: 'Prośba wysłana',
  Accepted: 'Zaakceptowana',
  Rejected: 'Odrzucona',
  Finished: 'Zakończona',
  AutoRejectedOnAccept: 'Odrzucona automatycznie (zaakceptowano inną prośbę)',
  AutoRejectedOnFinish: 'Odrzucona automatycznie (transakcja zakończona)',
}

/** Powiadomienie: opis dla osoby, która je dostaje (druga strona transakcji) */
export const notificationLabels: Record<string, string> = {
  Requested: 'Nowa prośba o Twoją książkę',
  Accepted: 'Twoja prośba została zaakceptowana',
  Rejected: 'Twoja prośba została odrzucona',
  Finished: 'Transakcja została zakończona',
  AutoRejectedOnAccept: 'Twoja prośba została odrzucona (zaakceptowano inną prośbę)',
  AutoRejectedOnFinish: 'Twoja prośba została odrzucona (transakcja zakończona)',
}

/** Opis typu; nieznany typ wraca bez zmian */
export function labelFor(labels: Record<string, string>, type: string): string {
  return labels[type] ?? type
}
