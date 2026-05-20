import { useQuery } from '@tanstack/vue-query'
import { type MaybeRef, computed, unref } from 'vue'
import useBackend from '~/features/kernel/composables/backend'
import type { PagedResponse } from '~/features/orden-surtimiento/model/common'
import type { SupplyOrderDto } from '~/features/orden-surtimiento/model/supply-order'

interface Params {
  pageNum: MaybeRef<number>
  search: MaybeRef<string>
}

export default function useSupplyOrdersQuery(params: Params) {
  const backend = useBackend()

  return useQuery({
    queryKey: computed(() => ['supply-orders', unref(params.pageNum), unref(params.search)]),
    queryFn: () =>
      backend.get<PagedResponse<SupplyOrderDto>>('SupplyOrders/GetOSInternoAsync', {
        params: {
          pageNum: unref(params.pageNum),
          search: unref(params.search),
        },
      }),
  })
}
