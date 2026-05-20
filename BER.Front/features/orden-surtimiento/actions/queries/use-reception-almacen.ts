import { useQuery } from '@tanstack/vue-query'
import { type MaybeRef, computed, unref } from 'vue'
import useBackend from '~/features/kernel/composables/backend'
import type { PagedResponse } from '~/features/orden-surtimiento/model/common'
import type { ReceptionDto } from '~/features/orden-surtimiento/model/reception-almacen'

interface Params {
  pageNum: MaybeRef<number>
  search: MaybeRef<string>
}

export default function useReceptionAlmacenQuery(params: Params) {
  const backend = useBackend()

  return useQuery({
    queryKey: computed(() => ['reception-almacen', unref(params.pageNum), unref(params.search)]),
    queryFn: () =>
      backend.get<PagedResponse<ReceptionDto>>('ReceptionAlmacen/GetReceptionAsync', {
        params: {
          pageNum: unref(params.pageNum),
          search: unref(params.search),
        },
      }),
  })
}
