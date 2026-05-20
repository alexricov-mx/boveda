import { useQuery } from '@tanstack/vue-query'
import { type MaybeRef, computed, unref } from 'vue'
import useBackend from '~/features/kernel/composables/backend'
import type { PagedResponse } from '~/features/orden-surtimiento/model/common'
import type { SOEstimationDto } from '~/features/orden-surtimiento/model/estimacion-obra'

interface Params {
  pageNum: MaybeRef<number>
}

export default function useEstimacionObraQuery(params: Params) {
  const backend = useBackend()

  return useQuery({
    queryKey: computed(() => ['estimacion-obra', unref(params.pageNum)]),
    queryFn: () =>
      backend.get<PagedResponse<SOEstimationDto>>('SOEstimation/GetSOEInternoAsync', {
        params: { pageNum: unref(params.pageNum) },
      }),
  })
}
