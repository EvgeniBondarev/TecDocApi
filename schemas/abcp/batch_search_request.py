from typing import List, Optional

from pydantic import BaseModel

from schemas.abcp.batch_search_item import BatchSearchItem


class BatchSearchRequest(BaseModel):
    search: List[BatchSearchItem]
    profileId: Optional[int] = None