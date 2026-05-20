import type { DateHelperModel } from './common'

export interface SOEstimationDto {
  estimacionID: string
  organismID: string
  contract: string
  sapOrder: string
  type: string
  documentType: string
  signer: string
  creditorNumber: string
  currency: string
  esTRI: boolean
  receptionDate: string
  representative: string
  total: string

  functionaryEmailReason: string
  functionaryEmails: string
  functionaryEmailSendDate: string | null
  functionaryNotifyPemexDate: string
  functionarySignDate: string | null

  providerEmailReason: string
  providerEmails: string
  providerEmailSendDate: string
  providerNotifyPemexDate: string
  providerSignDate: string | null

  isFullSigned: boolean
  isCancel: boolean
  cancelDate: string
  cancelBy: string

  organismName: string
  organismClave: string
  functionarySignerName: string
  providerSignerName: string
  functionaryCancelName: string
  seguimiento: DateHelperModel[]
}
