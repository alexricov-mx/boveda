import type { DateHelperModel } from './common'

export interface SupplyOrderDto {
  supplyOrderID: string
  organismID: string
  contract: string
  documentType: string
  sapOrder: string
  siafOrder: string
  type: string
  creditor: string
  creditorNumber: string
  creditorRFC: string
  total: string
  currency: string
  madeBy: string
  representative: string
  signer: string
  esTRI: boolean | null
  historical: boolean | null
  receptionDate: string | null

  administratorEmail: string
  administratorEmailSendDate: string | null
  administratorEmailReason: string
  administratorSignDate: string | null

  functionaryEmail: string
  functionaryEmailSendDate: string | null
  functionaryEmailReason: string
  functionarySignDate: string | null
  functionaryNotifyPemexDate: string | null
  functionaryFicha: string

  providerEmail: string
  providerEmailSendDate: string | null
  providerEmailReason: string
  providerSignDate: string | null
  providerNotifyPemexDate: string | null

  isFullSigned: boolean | null
  isCancel: boolean | null
  cancelDate: string | null
  cancelBy: string
  liberacionVPDate: string | null

  organismName: string
  organismClave: string
  functionarySignerName: string
  providerSignerName: string
  functionaryCancelName: string
  seguimiento: DateHelperModel[]
}
