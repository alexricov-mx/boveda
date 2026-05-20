export interface ReceptionDto {
  receptionID: string
  organismID: string
  contract: string
  sapOrder: string
  siafOrder: string
  reception: string
  receptionDate: string
  exercise: string
  ivaAmount: string
  amountWithIva: string
  amountWithoutIva: string
  briefText: string
  currency: string
  medicalUnit: string
  noExternal: string
  penalty: string
  provider: string
  receptionDateContab: string
  receptionDateRegistry: string
  signer: string
  total: string
  madeBy: string
  organismName: string
  clave: string

  emailFunctionary: boolean
  emailFunctionaryReason: string
  emailFunctionarySendDate: string
  functionary: boolean
  functionarySignDate: string

  notifyPemex: boolean
  notifyPemexDate: string
  providerEmail: string

  esTRI: boolean
  historical: boolean
}
